using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace IUIS.Infrastructure.Persistence
{
    public sealed class RepositoryEnvelopeMigrationAuditRecord
    {
        public string Id { get; set; }
        public string TransactionId { get; set; }
        public int MigratedRepositoryCount { get; set; }
        public IReadOnlyList<string> MigratedRepositoryNames { get; set; }
        public DateTime ExecutedAtUtc { get; set; }
        public string ExecutedByUserId { get; set; }
        public string Result { get; set; }
    }

    public sealed class RepositoryEnvelopeMigrationResult
    {
        public string TransactionId { get; set; }
        public int MigratedRepositoryCount { get; set; }
        public IReadOnlyList<string> MigratedRepositoryNames { get; set; }
        public string AuditRecordId { get; set; }
        public bool WasNoOp { get; set; }
    }

    public sealed class RepositoryEnvelopeMigrationService
    {
        private readonly ProductionRepositoryCatalog _catalog;
        private readonly JsonInfrastructureOptions _options;
        private readonly JsonRepositoryStore _store;
        private readonly JournaledTransactionCoordinator _transactions;
        private readonly JsonSerializerOptions _json = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public RepositoryEnvelopeMigrationService(
            ProductionRepositoryCatalog catalog,
            JsonInfrastructureOptions options)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _store = new JsonRepositoryStore(catalog, options);
            _transactions = new JournaledTransactionCoordinator(catalog, options);
        }

        public RepositoryEnvelopeMigrationResult MigrateAll(
            DateTime executedAtUtc,
            string executedByUserId)
        {
            if (executedAtUtc.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Migration timestamp must be UTC.", nameof(executedAtUtc));
            if (string.IsNullOrWhiteSpace(executedByUserId))
                throw new ArgumentException("Migration actor is required.", nameof(executedByUserId));

            var migratedNames = new List<string>();
            var mutations = new List<TransactionMutation>();
            foreach (var descriptor in _catalog.All)
            {
                if (string.Equals(
                    descriptor.Name,
                    "transaction_journal",
                    StringComparison.OrdinalIgnoreCase))
                    continue;

                var raw = _store.ReadRaw(descriptor.Name);
                if (!RepositoryEnvelopeJson.IsLegacy(raw)) continue;
                var envelope = RepositoryEnvelopeJson.Deserialize<JsonElement>(raw, _json);
                RepositoryEnvelopeJson.Validate(descriptor, envelope);
                envelope.RepositoryName = descriptor.Name;
                var canonical = RepositoryEnvelopeJson.Serialize(envelope, _json);
                mutations.Add(new TransactionMutation(
                    descriptor.Name,
                    canonical,
                    envelope.Revision,
                    true));
                migratedNames.Add(descriptor.Name);
            }

            var journalRaw = _store.ReadRaw("transaction_journal");
            var journalWasLegacy = RepositoryEnvelopeJson.IsLegacy(journalRaw);
            if (mutations.Count == 0 && journalWasLegacy)
            {
                var descriptor = _catalog.Get("operational_report_runs");
                var envelope = _store.Read<JsonElement>(descriptor.Name);
                mutations.Add(new TransactionMutation(
                    descriptor.Name,
                    RepositoryEnvelopeJson.Serialize(envelope, _json),
                    envelope.Revision,
                    true));
            }

            if (mutations.Count == 0)
            {
                return new RepositoryEnvelopeMigrationResult
                {
                    MigratedRepositoryCount = 0,
                    MigratedRepositoryNames = new string[0],
                    WasNoOp = true
                };
            }

            var transactionId = _transactions.Execute(mutations);
            if (journalWasLegacy) migratedNames.Add("transaction_journal");
            migratedNames = migratedNames
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var auditRecord = new RepositoryEnvelopeMigrationAuditRecord
            {
                Id = "REM-" + executedAtUtc.ToString("yyyyMMddHHmmss")
                    + "-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant(),
                TransactionId = transactionId,
                MigratedRepositoryCount = migratedNames.Count,
                MigratedRepositoryNames = migratedNames.AsReadOnly(),
                ExecutedAtUtc = executedAtUtc,
                ExecutedByUserId = executedByUserId.Trim(),
                Result = "Committed"
            };
            AppendAuditRecord(auditRecord);

            return new RepositoryEnvelopeMigrationResult
            {
                TransactionId = transactionId,
                MigratedRepositoryCount = migratedNames.Count,
                MigratedRepositoryNames = migratedNames.AsReadOnly(),
                AuditRecordId = auditRecord.Id,
                WasNoOp = false
            };
        }

        private void AppendAuditRecord(RepositoryEnvelopeMigrationAuditRecord record)
        {
            var envelope = _store.Read<JsonElement>("operational_report_runs");
            envelope.Records.Add(JsonSerializer.SerializeToElement(record, _json));
            envelope.UpdatedByUserId = record.ExecutedByUserId;
            _store.Write("operational_report_runs", envelope, envelope.Revision);
        }
    }
}
