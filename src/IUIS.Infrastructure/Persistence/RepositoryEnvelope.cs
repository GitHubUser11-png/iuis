using System;
using System.Collections.Generic;

namespace IUIS.Infrastructure.Persistence
{
    public sealed class RepositoryEnvelope<T>
    {
        public RepositoryEnvelope()
        {
            Repository = string.Empty;
            SchemaVersion = 1;
            Revision = 0;
            CreatedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = CreatedAtUtc;
            Records = new List<T>();
        }

        public string Repository { get; set; }
        public int SchemaVersion { get; set; }
        public long Revision { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public string UpdatedByUserId { get; set; }
        public List<T> Records { get; set; }
    }

    public sealed class RepositoryManifestRecord
    {
        public string Repository { get; set; }
        public string FileName { get; set; }
        public int SchemaVersion { get; set; }
        public long Revision { get; set; }
        public string Sha256 { get; set; }
        public DateTime VerifiedAtUtc { get; set; }
    }
}
