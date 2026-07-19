using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using IUIS.Domain.Identity;

namespace IUIS.Infrastructure.Persistence
{
    public sealed class IdSequenceRecord
    {
        public string Prefix { get; set; }
        public int Year { get; set; }
        public int LastAllocatedSequence { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public string UpdatedByUserId { get; set; }
    }

    public sealed class CentralIdSequenceService
    {
        private readonly ProductionRepositoryCatalog _catalog;
        private readonly JsonInfrastructureOptions _options;
        private readonly AtomicFileWriter _writer = new AtomicFileWriter();
        private readonly JsonSerializerOptions _json = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public CentralIdSequenceService(
            ProductionRepositoryCatalog catalog,
            JsonInfrastructureOptions options)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public string Allocate(string prefix, int year, string actorUserId)
        {
            prefix = NormalizePrefix(prefix);
            if (year < 2000 || year > 9999)
                throw new ArgumentOutOfRangeException(nameof(year));
            if (string.IsNullOrWhiteSpace(actorUserId))
                throw new ArgumentException("Actor user ID is required.", nameof(actorUserId));

            var path = _catalog.ResolvePath(_options.DataRoot, "id_sequences");
            using (CrossProcessFileLock.Acquire(path, _options.LockTimeout))
            {
                RepositoryEnvelope<IdSequenceRecord> envelope = null;
                if (File.Exists(path))
                {
                    envelope = JsonSerializer.Deserialize<RepositoryEnvelope<IdSequenceRecord>>(
                        File.ReadAllText(path),
                        _json);
                }

                if (envelope == null)
                {
                    envelope = new RepositoryEnvelope<IdSequenceRecord>
                    {
                        Repository = "id_sequences",
                        SchemaVersion = 1,
                        Revision = 0,
                        Records = new List<IdSequenceRecord>()
                    };
                }

                var record = envelope.Records.SingleOrDefault(item =>
                    string.Equals(item.Prefix, prefix, StringComparison.Ordinal)
                    && item.Year == year);
                if (record == null)
                {
                    record = new IdSequenceRecord
                    {
                        Prefix = prefix,
                        Year = year,
                        LastAllocatedSequence = 0
                    };
                    envelope.Records.Add(record);
                }

                record.LastAllocatedSequence = checked(record.LastAllocatedSequence + 1);
                record.UpdatedAtUtc = DateTime.UtcNow;
                record.UpdatedByUserId = actorUserId;
                envelope.Revision = checked(envelope.Revision + 1);
                envelope.UpdatedAtUtc = DateTime.UtcNow;
                envelope.UpdatedByUserId = actorUserId;
                _writer.WriteUtf8(path, JsonSerializer.Serialize(envelope, _json));

                return InstitutionIdentifier.Create(
                    prefix,
                    year,
                    record.LastAllocatedSequence).Value;
            }
        }

        private static string NormalizePrefix(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Identifier prefix is required.", nameof(value));

            var prefix = value.Trim().ToUpperInvariant();
            if (prefix.Length < 2
                || prefix.Length > 8
                || prefix.Any(character => character < 'A' || character > 'Z'))
            {
                throw new ArgumentException(
                    "Identifier prefix must contain 2 to 8 ASCII letters.",
                    nameof(value));
            }

            return prefix;
        }
    }
}
