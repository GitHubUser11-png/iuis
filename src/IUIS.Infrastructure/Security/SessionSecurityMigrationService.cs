using System;

using IUIS.Infrastructure.Persistence;

namespace IUIS.Infrastructure.Security
{
    public sealed class LegacySessionRevocationResult
    {
        public int LegacySessionCount { get; set; }
        public int RevokedActiveSessionCount { get; set; }
        public long ResultingRepositoryRevision { get; set; }
        public bool WasNoOp { get; set; }
    }

    public sealed class SessionSecurityMigrationService
    {
        private readonly JsonRepositoryStore _store;

        public SessionSecurityMigrationService(
            ProductionRepositoryCatalog catalog,
            JsonInfrastructureOptions options)
        {
            _store = new JsonRepositoryStore(
                catalog ?? throw new ArgumentNullException(nameof(catalog)),
                options ?? throw new ArgumentNullException(nameof(options)));
        }

        public LegacySessionRevocationResult RevokeLegacySessions(
            DateTime revokedAtUtc,
            string revokedByUserId)
        {
            if (revokedAtUtc.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Revocation timestamp must be UTC.", nameof(revokedAtUtc));
            if (string.IsNullOrWhiteSpace(revokedByUserId))
                throw new ArgumentException("Revocation actor is required.", nameof(revokedByUserId));

            var envelope = _store.Read<PersistedSessionRecord>("sessions");
            var legacyCount = 0;
            var revokedCount = 0;
            foreach (var session in envelope.Records)
            {
                var isLegacy = session.TokenDigestVersion
                        != SessionTokenProtector.CurrentDigestVersion
                    || string.IsNullOrWhiteSpace(session.TokenDigest)
                    || !string.IsNullOrWhiteSpace(session.LegacyTokenHash);
                if (!isLegacy) continue;
                legacyCount++;
                if (string.Equals(session.Status, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    session.Status = "Revoked";
                    session.RevokedAtUtc = revokedAtUtc;
                    revokedCount++;
                }
                session.TokenDigest = null;
                session.TokenDigestVersion = 0;
                session.LegacyTokenHash = null;
            }

            if (legacyCount == 0)
            {
                return new LegacySessionRevocationResult
                {
                    LegacySessionCount = 0,
                    RevokedActiveSessionCount = 0,
                    ResultingRepositoryRevision = envelope.Revision,
                    WasNoOp = true
                };
            }

            envelope.UpdatedByUserId = revokedByUserId.Trim();
            var expectedRevision = envelope.Revision;
            _store.Write("sessions", envelope, expectedRevision);
            return new LegacySessionRevocationResult
            {
                LegacySessionCount = legacyCount,
                RevokedActiveSessionCount = revokedCount,
                ResultingRepositoryRevision = checked(expectedRevision + 1),
                WasNoOp = false
            };
        }
    }
}
