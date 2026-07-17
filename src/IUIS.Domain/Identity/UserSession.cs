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
            : base(id, issuedAtUtc, createdByUserId)
        {
            UserId = DomainGuard.RequiredIdentifier(userId, nameof(userId));
            TokenHash = TextNormalizer.Required(tokenHash, nameof(tokenHash), 512);
            SecurityStampSnapshot = TextNormalizer.Required(securityStampSnapshot, nameof(securityStampSnapshot), 128);
            ApplicationKind = applicationKind;
            Purpose = purpose;
            Status = UserSessionStatus.Active;
            LastActivityAtUtc = issuedAtUtc;
            InactivityExpiresAtUtc = DomainGuard.RequireUtc(inactivityExpiresAtUtc, nameof(inactivityExpiresAtUtc));
            AbsoluteExpiresAtUtc = DomainGuard.RequireUtc(absoluteExpiresAtUtc, nameof(absoluteExpiresAtUtc));

            if (InactivityExpiresAtUtc <= issuedAtUtc || AbsoluteExpiresAtUtc <= issuedAtUtc)
            {
                throw new DomainValidationException("Session expiration must be later than its issue time.");
            }

            if (InactivityExpiresAtUtc > AbsoluteExpiresAtUtc)
            {
                throw new DomainValidationException("Inactivity expiration cannot exceed absolute expiration.");
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
                && utcNow < InactivityExpiresAtUtc
                && utcNow < AbsoluteExpiresAtUtc
                && StringComparer.Ordinal.Equals(SecurityStampSnapshot, currentSecurityStamp);
        }

        public void RecordActivity(DateTime activityAtUtc, DateTime newInactivityExpiresAtUtc, string changedByUserId)
        {
            DomainGuard.RequireUtc(activityAtUtc, nameof(activityAtUtc));
            DomainGuard.RequireUtc(newInactivityExpiresAtUtc, nameof(newInactivityExpiresAtUtc));
            if (newInactivityExpiresAtUtc > AbsoluteExpiresAtUtc || newInactivityExpiresAtUtc <= activityAtUtc)
            {
                throw new DomainValidationException("The new inactivity expiration is outside the valid session window.");
            }

            LastActivityAtUtc = activityAtUtc;
            InactivityExpiresAtUtc = newInactivityExpiresAtUtc;
            RecordChange(activityAtUtc, changedByUserId);
        }

        public void Revoke(DateTime revokedAtUtc, string revokedByUserId, string reason)
        {
            if (Status != UserSessionStatus.Active)
            {
                throw new DomainValidationException("Only active sessions can be revoked.");
            }

            RevocationReason = TextNormalizer.Required(reason, nameof(reason), 300);
            RevokedAtUtc = DomainGuard.RequireUtc(revokedAtUtc, nameof(revokedAtUtc));
            RevokedByUserId = DomainGuard.RequiredActorIdentifier(revokedByUserId, nameof(revokedByUserId));
            Status = UserSessionStatus.Revoked;
            RecordChange(revokedAtUtc, revokedByUserId);
        }
    }
}
