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
            _json = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public RepositoryEnvelope<T> Read<T>(string repositoryName)
        {
            var descriptor = _catalog.Get(repositoryName);
            var path = _catalog.ResolvePath(_options.DataRoot, repositoryName);
            using (CrossProcessFileLock.Acquire(path, _options.LockTimeout))
            {
                if (!File.Exists(path))
                    throw new FileNotFoundException("Production repository is missing.", path);
                var envelope = RepositoryEnvelopeJson.Deserialize<T>(
                    File.ReadAllText(path),
                    _json);
                RepositoryEnvelopeJson.Validate(descriptor, envelope);
                return envelope;
            }
        }

        public void Write<T>(
            string repositoryName,
            RepositoryEnvelope<T> envelope,
            long expectedRevision)
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));
            var descriptor = _catalog.Get(repositoryName);
            var path = _catalog.ResolvePath(_options.DataRoot, repositoryName);
            using (CrossProcessFileLock.Acquire(path, _options.LockTimeout))
            {
                if (File.Exists(path))
                {
                    var current = RepositoryEnvelopeJson.Deserialize<JsonElement>(
                        File.ReadAllText(path),
                        _json);
                    if (current == null || current.Revision != expectedRevision)
                        throw new InvalidOperationException(
                            "Repository revision conflict for " + repositoryName + ".");
                }
                else if (expectedRevision != -1)
                {
                    throw new InvalidOperationException(
                        "A missing repository requires expected revision -1.");
                }

                envelope.RepositoryName = descriptor.Name;
                envelope.SchemaVersion = descriptor.SchemaVersion;
                envelope.Revision = checked(expectedRevision + 1);
                envelope.UpdatedAtUtc = DateTime.UtcNow;
                RepositoryEnvelopeJson.Validate(descriptor, envelope);
                _writer.WriteUtf8(
                    path,
                    RepositoryEnvelopeJson.Serialize(envelope, _json));
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
            var descriptor = _catalog.Get(repositoryName);
            var path = _catalog.ResolvePath(_options.DataRoot, repositoryName);
            using (CrossProcessFileLock.Acquire(path, _options.LockTimeout))
            {
                var canonical = RepositoryEnvelopeJson.CanonicalizeRaw(
                    json,
                    descriptor,
                    _json);
                _writer.WriteUtf8(path, canonical);
            }
        }
    }
}
