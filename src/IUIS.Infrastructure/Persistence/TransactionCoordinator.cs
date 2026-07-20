using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace IUIS.Infrastructure.Persistence
{
    public enum TransactionJournalStatus
    {
        Prepared = 0,
        Applying = 1,
        Committed = 2,
        RolledBack = 3
    }

    public sealed class TransactionMutation
    {
        public TransactionMutation(string repositoryName, string completeJson)
            : this(repositoryName, completeJson, null)
        {
        }

        public TransactionMutation(
            string repositoryName,
            string completeJson,
            long expectedRevision)
            : this(repositoryName, completeJson, (long?)expectedRevision)
        {
        }

        private TransactionMutation(
            string repositoryName,
            string completeJson,
            long? expectedRevision)
        {
            if (string.IsNullOrWhiteSpace(repositoryName))
                throw new ArgumentException("Repository name is required.", nameof(repositoryName));
            if (string.IsNullOrWhiteSpace(completeJson))
                throw new ArgumentException("Complete JSON is required.", nameof(completeJson));
            if (expectedRevision.HasValue && expectedRevision.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(expectedRevision));

            using (JsonDocument.Parse(completeJson)) { }
            RepositoryName = repositoryName.Trim().ToLowerInvariant();
            CompleteJson = completeJson;
            ExpectedRevision = expectedRevision;
        }

        public string RepositoryName { get; private set; }
        public string CompleteJson { get; private set; }
        public long? ExpectedRevision { get; private set; }
    }

    public sealed class TransactionJournalEntry
    {
        public string RepositoryName { get; set; }
        public string TargetPath { get; set; }
        public string BackupPath { get; set; }
        public bool TargetExisted { get; set; }
        public long? ExpectedRevision { get; set; }
    }

    public sealed class TransactionJournalRecord
    {
        public string TransactionId { get; set; }
        public TransactionJournalStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public List<TransactionJournalEntry> Entries { get; set; }
    }

    public sealed class JournaledTransactionCoordinator
    {
        private readonly ProductionRepositoryCatalog _catalog;
        private readonly JsonInfrastructureOptions _options;
        private readonly AtomicFileWriter _writer = new AtomicFileWriter();
        private readonly JsonSerializerOptions _json = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public JournaledTransactionCoordinator(
            ProductionRepositoryCatalog catalog,
            JsonInfrastructureOptions options)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public string Execute(IEnumerable<TransactionMutation> mutations)
        {
            if (mutations == null) throw new ArgumentNullException(nameof(mutations));
            var items = mutations.ToList();
            if (items.Count == 0)
                throw new ArgumentException("At least one mutation is required.", nameof(mutations));
            if (items.Select(x => x.RepositoryName)
                .Distinct(StringComparer.OrdinalIgnoreCase).Count() != items.Count)
            {
                throw new InvalidOperationException(
                    "A transaction cannot mutate the same repository twice.");
            }

            foreach (var item in items) _catalog.Get(item.RepositoryName);

            var transactionId = Guid.NewGuid().ToString("N");
            var journalPath = _catalog.ResolvePath(_options.DataRoot, "transaction_journal");
            var lockPaths = items
                .Select(item => _catalog.ResolvePath(_options.DataRoot, item.RepositoryName))
                .Concat(new[] { journalPath })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var locks = AcquireAll(lockPaths);

            try
            {
                ValidateExpectedRevisionsLocked(items);

                var record = new TransactionJournalRecord
                {
                    TransactionId = transactionId,
                    Status = TransactionJournalStatus.Prepared,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow,
                    Entries = new List<TransactionJournalEntry>()
                };

                foreach (var item in items.OrderBy(
                    value => _catalog.ResolvePath(_options.DataRoot, value.RepositoryName),
                    StringComparer.OrdinalIgnoreCase))
                {
                    var target = _catalog.ResolvePath(_options.DataRoot, item.RepositoryName);
                    var backup = target + ".txn-" + transactionId + ".bak";
                    var existed = File.Exists(target);
                    if (existed) File.Copy(target, backup, true);

                    record.Entries.Add(new TransactionJournalEntry
                    {
                        RepositoryName = item.RepositoryName,
                        TargetPath = target,
                        BackupPath = backup,
                        TargetExisted = existed,
                        ExpectedRevision = item.ExpectedRevision
                    });
                }

                WriteJournal(journalPath, record);
                record.Status = TransactionJournalStatus.Applying;
                record.UpdatedAtUtc = DateTime.UtcNow;
                WriteJournal(journalPath, record);

                foreach (var item in items)
                {
                    _writer.WriteUtf8(
                        _catalog.ResolvePath(_options.DataRoot, item.RepositoryName),
                        item.CompleteJson);
                }

                record.Status = TransactionJournalStatus.Committed;
                record.UpdatedAtUtc = DateTime.UtcNow;
                WriteJournal(journalPath, record);
                DeleteBackups(record);
                return transactionId;
            }
            catch
            {
                TryRollbackLocked(journalPath, transactionId);
                throw;
            }
            finally
            {
                DisposeAll(locks);
            }
        }

        public bool RecoverIncompleteTransaction()
        {
            var journalPath = _catalog.ResolvePath(_options.DataRoot, "transaction_journal");
            if (!File.Exists(journalPath)) return false;

            TransactionJournalRecord snapshot;
            using (CrossProcessFileLock.Acquire(journalPath, _options.LockTimeout))
            {
                snapshot = ReadJournal(journalPath);
            }

            if (snapshot == null
                || snapshot.Status == TransactionJournalStatus.Committed
                || snapshot.Status == TransactionJournalStatus.RolledBack)
            {
                return false;
            }

            var lockPaths = snapshot.Entries
                .Select(entry => entry.TargetPath)
                .Concat(new[] { journalPath })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var locks = AcquireAll(lockPaths);
            try
            {
                var current = ReadJournal(journalPath);
                if (current == null
                    || current.Status == TransactionJournalStatus.Committed
                    || current.Status == TransactionJournalStatus.RolledBack)
                {
                    return false;
                }

                Restore(current);
                current.Status = TransactionJournalStatus.RolledBack;
                current.UpdatedAtUtc = DateTime.UtcNow;
                WriteJournal(journalPath, current);
                DeleteBackups(current);
                return true;
            }
            finally
            {
                DisposeAll(locks);
            }
        }

        private void ValidateExpectedRevisionsLocked(
            IEnumerable<TransactionMutation> mutations)
        {
            foreach (var mutation in mutations.Where(item => item.ExpectedRevision.HasValue))
            {
                var targetPath = _catalog.ResolvePath(
                    _options.DataRoot,
                    mutation.RepositoryName);
                if (!File.Exists(targetPath))
                {
                    throw new InvalidOperationException(
                        "A revision-checked transaction requires the authoritative repository "
                        + mutation.RepositoryName + ".");
                }

                var current = JsonSerializer.Deserialize<RepositoryEnvelope<JsonElement>>(
                    File.ReadAllText(targetPath),
                    _json);
                if (current == null || current.Records == null)
                {
                    throw new InvalidDataException(
                        "Repository envelope is invalid for " + mutation.RepositoryName + ".");
                }

                if (current.Revision != mutation.ExpectedRevision.Value)
                {
                    throw new InvalidOperationException(
                        "Repository revision conflict for " + mutation.RepositoryName + ".");
                }

                var staged = JsonSerializer.Deserialize<RepositoryEnvelope<JsonElement>>(
                    mutation.CompleteJson,
                    _json);
                if (staged == null
                    || staged.Records == null
                    || !string.Equals(
                        staged.Repository,
                        mutation.RepositoryName,
                        StringComparison.OrdinalIgnoreCase)
                    || staged.Revision != checked(mutation.ExpectedRevision.Value + 1))
                {
                    throw new InvalidDataException(
                        "The staged repository envelope is inconsistent for "
                        + mutation.RepositoryName + ".");
                }
            }
        }

        private List<CrossProcessFileLock> AcquireAll(IEnumerable<string> paths)
        {
            var locks = new List<CrossProcessFileLock>();
            try
            {
                foreach (var path in paths)
                    locks.Add(CrossProcessFileLock.Acquire(path, _options.LockTimeout));
                return locks;
            }
            catch
            {
                DisposeAll(locks);
                throw;
            }
        }

        private static void DisposeAll(IList<CrossProcessFileLock> locks)
        {
            for (var index = locks.Count - 1; index >= 0; index--)
                locks[index].Dispose();
        }

        private void TryRollbackLocked(string journalPath, string transactionId)
        {
            try
            {
                if (!File.Exists(journalPath)) return;
                var record = ReadJournal(journalPath);
                if (record == null
                    || !string.Equals(record.TransactionId, transactionId, StringComparison.Ordinal))
                {
                    return;
                }

                Restore(record);
                record.Status = TransactionJournalStatus.RolledBack;
                record.UpdatedAtUtc = DateTime.UtcNow;
                WriteJournal(journalPath, record);
                DeleteBackups(record);
            }
            catch
            {
                // Recovery remains possible from retained journal and backup files.
            }
        }

        private static void Restore(TransactionJournalRecord record)
        {
            foreach (var entry in record.Entries)
            {
                if (entry.TargetExisted && File.Exists(entry.BackupPath))
                    File.Copy(entry.BackupPath, entry.TargetPath, true);
                else if (!entry.TargetExisted && File.Exists(entry.TargetPath))
                    File.Delete(entry.TargetPath);
            }
        }

        private static void DeleteBackups(TransactionJournalRecord record)
        {
            foreach (var entry in record.Entries)
            {
                if (!string.IsNullOrWhiteSpace(entry.BackupPath)
                    && File.Exists(entry.BackupPath))
                {
                    File.Delete(entry.BackupPath);
                }
            }
        }

        private TransactionJournalRecord ReadJournal(string path)
        {
            var envelope = JsonSerializer.Deserialize<RepositoryEnvelope<TransactionJournalRecord>>(
                File.ReadAllText(path),
                _json);
            return envelope == null || envelope.Records == null || envelope.Records.Count == 0
                ? null
                : envelope.Records[envelope.Records.Count - 1];
        }

        private void WriteJournal(string path, TransactionJournalRecord record)
        {
            RepositoryEnvelope<TransactionJournalRecord> envelope = null;
            if (File.Exists(path))
            {
                envelope = JsonSerializer.Deserialize<RepositoryEnvelope<TransactionJournalRecord>>(
                    File.ReadAllText(path),
                    _json);
            }

            if (envelope == null)
            {
                envelope = new RepositoryEnvelope<TransactionJournalRecord>
                {
                    Repository = "transaction_journal",
                    SchemaVersion = 1,
                    Revision = 0,
                    Records = new List<TransactionJournalRecord>()
                };
            }

            envelope.Records.RemoveAll(
                item => string.Equals(item.TransactionId, record.TransactionId, StringComparison.Ordinal));
            envelope.Records.Add(record);
            envelope.Revision = checked(envelope.Revision + 1);
            envelope.UpdatedAtUtc = DateTime.UtcNow;
            _writer.WriteUtf8(path, JsonSerializer.Serialize(envelope, _json));
        }
    }
}
