using System;

using IUIS.Domain.Common;

namespace IUIS.Domain.Identity
{
    public sealed class UserSession : EntityBase
    {
        private UserSession()
        {
            UserId = string.Empty;
            TokenHash = string.Empty;
            SecurityStampSnapshot = string.Empty;
        }

        public UserSession(
            string id,
            string userId,
            string tokenHash,
            string securityStampSnapshot,
            SessionApplicationKind applicationKind,
            SessionPurpose purpose,
            DateTime issuedAtUtc,
            DateTime inactivityExpiresAtUtc,
            DateTime absoluteExpiresAtUtc,
            string createdByUserId)
            : base(RequireIdentifier(id, "SES", "Session"), issuedAtUtc, createdByUserId)
        {
            UserId = RequireIdentifier(userId, "USR", "User");
            TokenHash = TextNormalizer.Required(tokenHash, nameof(tokenHash), 512);
            SecurityStampSnapshot = TextNormalizer.Required(
                securityStampSnapshot,
                nameof(securityStampSnapshot),
                128);
            ApplicationKind = RequireApplicationKind(applicationKind);
            Purpose = RequirePurpose(purpose);
            Status = UserSessionStatus.Active;
            LastActivityAtUtc = issuedAtUtc;
            InactivityExpiresAtUtc = DomainGuard.RequireUtc(
                inactivityExpiresAtUtc,
                nameof(inactivityExpiresAtUtc));
            AbsoluteExpiresAtUtc = DomainGuard.RequireUtc(
                absoluteExpiresAtUtc,
                nameof(absoluteExpiresAtUtc));

            if (InactivityExpiresAtUtc <= issuedAtUtc
                || AbsoluteExpiresAtUtc <= issuedAtUtc)
            {
                throw new DomainValidationException(
                    "Session expiration must be later than its issue time.");
            }

            if (InactivityExpiresAtUtc > AbsoluteExpiresAtUtc)
            {
                throw new DomainValidationException(
                    "Inactivity expiration cannot exceed absolute expiration.");
            }
        }

        public string UserId { get; private set; }

        public string TokenHash { get; private set; }

        public string SecurityStampSnapshot { get; private set; }

        public SessionApplicationKind ApplicationKind { get; private set; }

        public SessionPurpose Purpose { get; private set; }

        public UserSessionStatus Status { get; private set; }

        public DateTime LastActivityAtUtc { get; private set; }

        public DateTime InactivityExpiresAtUtc { get; private set; }

        public DateTime AbsoluteExpiresAtUtc { get; private set; }

        public DateTime? RevokedAtUtc { get; private set; }

        public string RevokedByUserId { get; private set; }

        public string RevocationReason { get; private set; }

        public bool IsUsableAt(DateTime utcNow, string currentSecurityStamp)
        {
            DomainGuard.RequireUtc(utcNow, nameof(utcNow));
            return Status == UserSessionStatus.Active
                && utcNow >= CreatedAtUtc
                && utcNow < InactivityExpiresAtUtc
                && utcNow < AbsoluteExpiresAtUtc
                && StringComparer.Ordinal.Equals(
                    SecurityStampSnapshot,
                    currentSecurityStamp);
        }

        public void RecordActivity(
            DateTime activityAtUtc,
            DateTime newInactivityExpiresAtUtc,
            string changedByUserId)
        {
            EnsureActive();
            DomainGuard.RequireChronological(
                LastActivityAtUtc,
                activityAtUtc,
                nameof(activityAtUtc));
            DomainGuard.RequireUtc(
                newInactivityExpiresAtUtc,
                nameof(newInactivityExpiresAtUtc));

            if (activityAtUtc >= AbsoluteExpiresAtUtc
                || newInactivityExpiresAtUtc > AbsoluteExpiresAtUtc
                || newInactivityExpiresAtUtc <= activityAtUtc)
            {
                throw new DomainValidationException(
                    "The new inactivity expiration is outside the valid session window.");
            }

            RecordChange(activityAtUtc, changedByUserId);
            LastActivityAtUtc = activityAtUtc;
            InactivityExpiresAtUtc = newInactivityExpiresAtUtc;
        }

        public void Revoke(
            DateTime revokedAtUtc,
            string revokedByUserId,
            string reason)
        {
            EnsureActive();
            var normalizedReason = TextNormalizer.Required(
                reason,
                nameof(reason),
                300);
            var normalizedActorId = DomainGuard.RequiredActorIdentifier(
                revokedByUserId,
                nameof(revokedByUserId));
            DomainGuard.RequireUtc(revokedAtUtc, nameof(revokedAtUtc));

            RecordChange(revokedAtUtc, normalizedActorId);
            Status = UserSessionStatus.Revoked;
            RevokedAtUtc = revokedAtUtc;
            RevokedByUserId = normalizedActorId;
            RevocationReason = normalizedReason;
        }

        private void EnsureActive()
        {
            if (Status != UserSessionStatus.Active)
            {
                throw new DomainValidationException(
                    "Only active sessions can be changed.");
            }
        }

        private static SessionApplicationKind RequireApplicationKind(
            SessionApplicationKind value)
        {
            if (!Enum.IsDefined(typeof(SessionApplicationKind), value)
                || value == SessionApplicationKind.Unspecified)
            {
                throw new DomainValidationException(
                    "Session application kind is invalid.");
            }

            return value;
        }

        private static SessionPurpose RequirePurpose(SessionPurpose value)
        {
            if (!Enum.IsDefined(typeof(SessionPurpose), value)
                || value == SessionPurpose.Unspecified)
            {
                throw new DomainValidationException("Session purpose is invalid.");
            }

            return value;
        }

        private static string RequireIdentifier(
            string value,
            string prefix,
            string entityName)
        {
            var identifier = InstitutionIdentifier.Parse(value);
            if (!StringComparer.Ordinal.Equals(identifier.Prefix, prefix))
            {
                throw new DomainValidationException(
                    entityName + " IDs must use the " + prefix + " prefix.");
            }

            return identifier.Value;
        }
    }
}
