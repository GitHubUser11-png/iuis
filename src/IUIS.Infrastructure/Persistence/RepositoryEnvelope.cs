using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace IUIS.Infrastructure.Persistence
{
    [JsonConverter(typeof(RepositoryEnvelopeJsonConverterFactory))]
    public sealed class RepositoryEnvelope<T>
    {
        public RepositoryEnvelope()
        {
            RepositoryName = string.Empty;
            SchemaVersion = 1;
            Revision = 0;
            UpdatedAtUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            UpdatedByUserId = "SYSTEM";
            Records = new List<T>();
        }

        public string RepositoryName { get; set; }
        public int SchemaVersion { get; set; }
        public long Revision { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public string UpdatedByUserId { get; set; }
        public List<T> Records { get; set; }

        [JsonIgnore]
        public string Repository
        {
            get { return RepositoryName; }
            set { RepositoryName = value; }
        }

        [JsonIgnore]
        public DateTime CreatedAtUtc { get; set; }
    }

    [JsonConverter(typeof(RepositoryManifestRecordJsonConverter))]
    public sealed class RepositoryManifestRecord
    {
        public string RepositoryName { get; set; }
        public string FileName { get; set; }
        public int SchemaVersion { get; set; }
        public long Revision { get; set; }
        public string Sha256 { get; set; }
        public DateTime VerifiedAtUtc { get; set; }

        [JsonIgnore]
        public string Repository
        {
            get { return RepositoryName; }
            set { RepositoryName = value; }
        }
    }
}
