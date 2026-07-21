using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace IUIS.Infrastructure.Persistence
{
    public enum RepositoryEnvelopeMigrationAuditStatus
    {
        NotRequired = 0,
        Registered = 1,
        RecoveryRequired = 2
    }

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
        public DateTime ExecutedAtUtc { get; set; }
        public string ExecutedByUserId { get; set; }
        public RepositoryEnvelopeMigrationAuditStatus AuditStatus { get; set; }
        public bool WasNoOp { get; set; }
    }

    public interface IRepositoryEnvelopeMigrationAuditFailureInjector
    {
        void BeforeAuditRegistration(
            RepositoryEnvelopeMigrationAuditRecord record);
    }

    public sealed class RepositoryEnvelopeMigrationAuditRegistrationException
        : InvalidOperationException
    {
        public RepositoryEnvelopeMigrationAuditRegistrationException(
            RepositoryEnvelopeMigrationResult result,
            Exception innerException)
            : base(
                "Repository-envelope migration committed, but audit registration requires recovery.",
                innerException)
        {
            Result = result ?? throw new ArgumentNullException(nameof(result));
        }

        public RepositoryEnvelopeMigrationResult Result { get; private set; }
    }

    public sealed class RepositoryEnvelopeMigrationService
    {
        private readonly ProductionRepositoryCatalog _catalog;
        private readonly JsonInfrastructureOptions _options;
        private readonly JsonRepositoryStore _store;
        private readonly JournaledTransactionCoordinator _transactions;
        private readonly IRepositoryEnvelopeMigrationAuditFailureInjector
            _auditFailureInjector;
        private readonly JsonSerializerOptions _json = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public RepositoryEnvelopeMigrationService(
            ProductionRepositoryCatalog catalog,
            JsonInfrastructureOptions options)
            : this(catalog, options, null, null)
        {
        }

        public RepositoryEnvelopeMigrationService(
            ProductionRepositoryCatalog catalog,
            JsonInfrastructureOptions options,
            ITransactionFailureInjector transactionFailureInjector,
            IRepositoryEnvelopeMigrationAuditFailureInjector
                auditFailureInjector)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _store = new JsonRepositoryStore(catalog, options);
            _transactions = new JournaledTransactionCoordinator(
                catalog,
                options,
                transactionFailureInjector);
            _auditFailureInjector = auditFailureInjector;
        }

        public RepositoryEnvelopeMigrationResult MigrateAll(
            DateTime executedAtUtc,
            string executedByUserId)
        {
            ValidateExecution(executedAtUtc, executedByUserId);
            var canonicalActor = executedByUserId.Trim();
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
                var envelope = RepositoryEnvelopeJson.Deserialize<JsonElement>(
                    raw,
                    _json);
                RepositoryEnvelopeJson.Validate(descriptor, envelope);
                envelope.RepositoryName = descriptor.Name;
                var canonical = RepositoryEnvelopeJson.Serialize(
                    envelope,
                    _json);
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
                    ExecutedAtUtc = executedAtUtc,
                    ExecutedByUserId = canonicalActor,
                    AuditStatus = RepositoryEnvelopeMigrationAuditStatus.NotRequired,
                    WasNoOp = true
                };
            }

            var transactionId = _transactions.Execute(mutations);
            if (journalWasLegacy) migratedNames.Add("transaction_journal");
            migratedNames = migratedNames
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var result = new RepositoryEnvelopeMigrationResult
            {
                TransactionId = transactionId,
                MigratedRepositoryCount = migratedNames.Count,
                MigratedRepositoryNames = migratedNames.AsReadOnly(),
                AuditRecordId = "REM-" + executedAtUtc.ToString("yyyyMMddHHmmss")
                    + "-" + Guid.NewGuid().ToString("N")
                        .Substring(0, 8)
                        .ToUpperInvariant(),
                ExecutedAtUtc = executedAtUtc,
                ExecutedByUserId = canonicalActor,
                AuditStatus = RepositoryEnvelopeMigrationAuditStatus.RecoveryRequired,
                WasNoOp = false
            };
            var auditRecord = CreateAuditRecord(result);

            try
            {
                if (_auditFailureInjector != null)
                    _auditFailureInjector.BeforeAuditRegistration(auditRecord);
                AppendAuditRecordIfMissing(auditRecord);
                result.AuditStatus = RepositoryEnvelopeMigrationAuditStatus.Registered;
            }
            catch (Exception exception)
            {
                result.AuditStatus =
                    RepositoryEnvelopeMigrationAuditStatus.RecoveryRequired;
                throw new RepositoryEnvelopeMigrationAuditRegistrationException(
                    result,
                    exception);
            }

            return result;
        }

        public RepositoryEnvelopeMigrationResult RecoverAuditRegistration(
            RepositoryEnvelopeMigrationResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (result.WasNoOp)
            {
                result.AuditStatus =
                    RepositoryEnvelopeMigrationAuditStatus.NotRequired;
                return result;
            }
            if (string.IsNullOrWhiteSpace(result.TransactionId)
                || string.IsNullOrWhiteSpace(result.AuditRecordId)
                || result.MigratedRepositoryNames == null
                || result.MigratedRepositoryCount
                    != result.MigratedRepositoryNames.Count)
                throw new ArgumentException(
                    "Migration recovery evidence is incomplete.",
                    nameof(result));
            ValidateExecution(
                result.ExecutedAtUtc,
                result.ExecutedByUserId);

            AppendAuditRecordIfMissing(CreateAuditRecord(result));
            result.AuditStatus =
                RepositoryEnvelopeMigrationAuditStatus.Registered;
            return result;
        }

        private static void ValidateExecution(
            DateTime executedAtUtc,
            string executedByUserId)
        {
            if (executedAtUtc.Kind != DateTimeKind.Utc)
                throw new ArgumentException(
                    "Migration timestamp must be UTC.",
                    nameof(executedAtUtc));
            if (string.IsNullOrWhiteSpace(executedByUserId))
                throw new ArgumentException(
                    "Migration actor is required.",
                    nameof(executedByUserId));
        }

        private static RepositoryEnvelopeMigrationAuditRecord
            CreateAuditRecord(RepositoryEnvelopeMigrationResult result)
        {
            return new RepositoryEnvelopeMigrationAuditRecord
            {
                Id = result.AuditRecordId,
                TransactionId = result.TransactionId,
                MigratedRepositoryCount = result.MigratedRepositoryCount,
                MigratedRepositoryNames = result.MigratedRepositoryNames,
                ExecutedAtUtc = result.ExecutedAtUtc,
                ExecutedByUserId = result.ExecutedByUserId,
                Result = "Committed"
            };
        }

        private void AppendAuditRecordIfMissing(
            RepositoryEnvelopeMigrationAuditRecord record)
        {
            var envelope = _store.Read<JsonElement>(
                "operational_report_runs");
            foreach (var item in envelope.Records)
            {
                string id;
                string transactionId;
                if (TryReadString(item, "id", out id)
                    && TryReadString(
                        item,
                        "transactionId",
                        out transactionId)
                    && (StringComparer.Ordinal.Equals(id, record.Id)
                        || StringComparer.Ordinal.Equals(
                            transactionId,
                            record.TransactionId)))
                    return;
            }

            envelope.Records.Add(JsonSerializer.SerializeToElement(
                record,
                _json));
            envelope.UpdatedByUserId = record.ExecutedByUserId;
            _store.Write(
                "operational_report_runs",
                envelope,
                envelope.Revision);
        }

        private static bool TryReadString(
            JsonElement element,
            string propertyName,
            out string value)
        {
            value = null;
            JsonElement property;
            if (element.ValueKind != JsonValueKind.Object
                || !element.TryGetProperty(propertyName, out property)
                || property.ValueKind != JsonValueKind.String)
                return false;
            value = property.GetString();
            return !string.IsNullOrWhiteSpace(value);
        }
    }
}
