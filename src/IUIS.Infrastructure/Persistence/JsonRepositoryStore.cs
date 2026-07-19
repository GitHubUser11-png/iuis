using System;
using System.IO;
using System.Text.Json;

namespace IUIS.Infrastructure.Persistence
{
    public sealed class JsonRepositoryStore
    {
        private readonly ProductionRepositoryCatalog _catalog;
        private readonly JsonInfrastructureOptions _options;
        private readonly AtomicFileWriter _writer;
        private readonly JsonSerializerOptions _json;

        public JsonRepositoryStore(ProductionRepositoryCatalog catalog, JsonInfrastructureOptions options)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _writer = new AtomicFileWriter();
            _json = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        }

        public RepositoryEnvelope<T> Read<T>(string repositoryName)
        {
            var descriptor = _catalog.Get(repositoryName);
            var path = _catalog.ResolvePath(_options.DataRoot, repositoryName);
            using (CrossProcessFileLock.Acquire(path, _options.LockTimeout))
            {
                if (!File.Exists(path)) throw new FileNotFoundException("Production repository is missing.", path);
                var envelope = JsonSerializer.Deserialize<RepositoryEnvelope<T>>(File.ReadAllText(path), _json);
                ValidateEnvelope(descriptor, envelope);
                return envelope;
            }
        }

        public void Write<T>(string repositoryName, RepositoryEnvelope<T> envelope, long expectedRevision)
        {
            var descriptor = _catalog.Get(repositoryName);
            var path = _catalog.ResolvePath(_options.DataRoot, repositoryName);
            using (CrossProcessFileLock.Acquire(path, _options.LockTimeout))
            {
                if (File.Exists(path))
                {
                    var current = JsonSerializer.Deserialize<RepositoryEnvelope<JsonElement>>(File.ReadAllText(path), _json);
                    if (current == null || current.Revision != expectedRevision)
                        throw new InvalidOperationException("Repository revision conflict for " + repositoryName + ".");
                }
                else if (expectedRevision != -1)
                {
                    throw new InvalidOperationException("A missing repository requires expected revision -1.");
                }

                ValidateEnvelope(descriptor, envelope);
                envelope.Revision = checked(expectedRevision + 1);
                envelope.UpdatedAtUtc = DateTime.UtcNow;
                _writer.WriteUtf8(path, JsonSerializer.Serialize(envelope, _json));
            }
        }

        public string ReadRaw(string repositoryName)
        {
            var path = _catalog.ResolvePath(_options.DataRoot, repositoryName);
            using (CrossProcessFileLock.Acquire(path, _options.LockTimeout))
                return File.ReadAllText(path);
        }

        public void WriteRaw(string repositoryName, string json)
        {
            var path = _catalog.ResolvePath(_options.DataRoot, repositoryName);
            using (CrossProcessFileLock.Acquire(path, _options.LockTimeout))
            {
                using (JsonDocument.Parse(json)) { }
                _writer.WriteUtf8(path, json);
            }
        }

        private static void ValidateEnvelope<T>(ProductionRepositoryDescriptor descriptor, RepositoryEnvelope<T> envelope)
        {
            if (envelope == null) throw new InvalidDataException("Repository envelope is missing.");
            if (!string.Equals(envelope.Repository, descriptor.Name, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Repository envelope name does not match its catalog entry.");
            if (envelope.SchemaVersion != descriptor.SchemaVersion)
                throw new InvalidDataException("Repository schema version is unsupported.");
            if (envelope.Revision < 0) throw new InvalidDataException("Repository revision cannot be negative.");
            if (envelope.Records == null) throw new InvalidDataException("Repository records collection is required.");
        }
    }
}
