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
            EndedByUserId = string.Empty;
        }

        public UserSession(
            string id,
            string userId,
            PrimaryRole roleSnapshot,
            SessionApplicationKind applicationKind,
            SessionPurpose purpose,
            string tokenHash,
            string securityStampSnapshot,
            DateTime startedAtUtc,
            DateTime inactivityExpiresAtUtc,
            DateTime absoluteExpiresAtUtc,
            string createdByUserId)
            : base(
                IdentifierPolicy.Require(id, IdentifierPrefixes.Session, nameof(id)),
                startedAtUtc,
                createdByUserId)
        {
            UserId = IdentifierPolicy.Require(
                userId,
                IdentifierPrefixes.User,
                nameof(userId));
            RoleSnapshot = RequireRole(roleSnapshot, nameof(roleSnapshot));
            ApplicationKind = RequireApplicationKind(applicationKind, nameof(applicationKind));
            Purpose = RequirePurpose(purpose, nameof(purpose));

            if (!IdentityPolicy.IsCompatible(RoleSnapshot, ApplicationKind))
            {
                throw new DomainValidationException(
                    "The role snapshot is not compatible with the selected application.");
            }

            TokenHash = RequireOpaqueValue(tokenHash, nameof(tokenHash), 16, 512);
            SecurityStampSnapshot = RequireOpaqueValue(
                securityStampSnapshot,
                nameof(securityStampSnapshot),
                16,
                256);

            StartedAtUtc = startedAtUtc;
            LastActivityAtUtc = startedAtUtc;
            InactivityExpiresAtUtc = RequireUtcAfter(
                inactivityExpiresAtUtc,
                startedAtUtc,
                nameof(inactivityExpiresAtUtc));
            AbsoluteExpiresAtUtc = RequireUtcAfter(
                absoluteExpiresAtUtc,
                startedAtUtc,
                nameof(absoluteExpiresAtUtc));

            if (InactivityExpiresAtUtc > AbsoluteExpiresAtUtc)
            {
                throw new DomainValidationException(
                    nameof(inactivityExpiresAtUtc) + " cannot be later than the absolute expiration.");
            }

            Status = UserSessionStatus.Active;
        }

        public string UserId { get; private set; }

        public PrimaryRole RoleSnapshot { get; private set; }

        public SessionApplicationKind ApplicationKind { get; private set; }

        public SessionPurpose Purpose { get; private set; }

        public UserSessionStatus Status { get; private set; }

        public string TokenHash { get; private set; }

        public string SecurityStampSnapshot { get; private set; }

        public DateTime StartedAtUtc { get; private set; }

        public DateTime LastActivityAtUtc { get; private set; }

        public DateTime InactivityExpiresAtUtc { get; private set; }

        public DateTime AbsoluteExpiresAtUtc { get; private set; }

        public DateTime? EndedAtUtc { get; private set; }

        public string EndedByUserId { get; private set; }

        public string EndReason { get; private set; }

        public DateTime EffectiveExpiresAtUtc
        {
            get
            {
                return InactivityExpiresAtUtc <= AbsoluteExpiresAtUtc
                    ? InactivityExpiresAtUtc
                    : AbsoluteExpiresAtUtc;
            }
        }

        public bool IsUsableAt(DateTime checkedAtUtc)
        {
            DomainGuard.RequireUtc(checkedAtUtc, nameof(checkedAtUtc));

            return Status == UserSessionStatus.Active
                && !IsArchived
                && checkedAtUtc >= StartedAtUtc
                && checkedAtUtc < InactivityExpiresAtUtc
                && checkedAtUtc < AbsoluteExpiresAtUtc;
        }

        public bool HasCurrentSecurityStamp(string currentSecurityStamp)
        {
            var normalized = RequireOpaqueValue(
                currentSecurityStamp,
                nameof(currentSecurityStamp),
                16,
                256);
            return StringComparer.Ordinal.Equals(SecurityStampSnapshot, normalized);
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

            if (activityAtUtc >= AbsoluteExpiresAtUtc)
            {
                throw new DomainValidationException(
                    nameof(activityAtUtc) + " must be earlier than the absolute expiration.");
            }

            var normalizedInactivityExpiration = RequireUtcAfter(
                newInactivityExpiresAtUtc,
                activityAtUtc,
                nameof(newInactivityExpiresAtUtc));
            if (normalizedInactivityExpiration > AbsoluteExpiresAtUtc)
            {
                throw new DomainValidationException(
                    nameof(newInactivityExpiresAtUtc) + " cannot exceed the absolute expiration.");
            }

            RecordChange(activityAtUtc, changedByUserId);
            LastActivityAtUtc = activityAtUtc;
            InactivityExpiresAtUtc = normalizedInactivityExpiration;
        }

        public void Revoke(
            DateTime revokedAtUtc,
            string revokedByUserId,
            string reason)
        {
            EnsureActive();
            var normalizedReason = TextNormalizer.Required(reason, nameof(reason), 250);

            RecordChange(revokedAtUtc, revokedByUserId);
            Status = UserSessionStatus.Revoked;
            EndedAtUtc = revokedAtUtc;
            EndedByUserId = revokedByUserId;
            EndReason = normalizedReason;
        }

        public void MarkExpired(DateTime expiredAtUtc, string changedByUserId)
        {
            EnsureActive();
            DomainGuard.RequireUtc(expiredAtUtc, nameof(expiredAtUtc));

            if (expiredAtUtc < EffectiveExpiresAtUtc)
            {
                throw new DomainValidationException(
                    nameof(expiredAtUtc) + " cannot precede the effective session expiration.");
            }

            RecordChange(expiredAtUtc, changedByUserId);
            Status = UserSessionStatus.Expired;
            EndedAtUtc = expiredAtUtc;
            EndedByUserId = changedByUserId;
            EndReason = "Session expiration recorded.";
        }

        private static PrimaryRole RequireRole(PrimaryRole role, string parameterName)
        {
            if (role == PrimaryRole.Unspecified
                || !Enum.IsDefined(typeof(PrimaryRole), role))
            {
                throw new DomainValidationException(parameterName + " is invalid.");
            }

            return role;
        }

        private static SessionApplicationKind RequireApplicationKind(
            SessionApplicationKind applicationKind,
            string parameterName)
        {
            if (applicationKind == SessionApplicationKind.Unspecified
                || !Enum.IsDefined(typeof(SessionApplicationKind), applicationKind))
            {
                throw new DomainValidationException(parameterName + " is invalid.");
            }

            return applicationKind;
        }

        private static SessionPurpose RequirePurpose(
            SessionPurpose purpose,
            string parameterName)
        {
            if (purpose == SessionPurpose.Unspecified
                || !Enum.IsDefined(typeof(SessionPurpose), purpose))
            {
                throw new DomainValidationException(parameterName + " is invalid.");
            }

            return purpose;
        }

        private static DateTime RequireUtcAfter(
            DateTime value,
            DateTime precedingUtc,
            string parameterName)
        {
            DomainGuard.RequireUtc(value, parameterName);
            DomainGuard.RequireUtc(precedingUtc, nameof(precedingUtc));

            if (value <= precedingUtc)
            {
                throw new DomainValidationException(
                    parameterName + " must be later than the preceding timestamp.");
            }

            return value;
        }

        private static string RequireOpaqueValue(
            string value,
            string parameterName,
            int minimumLength,
            int maximumLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainValidationException(parameterName + " is required.");
            }

            var normalized = value.Trim();
            if (normalized.Length < minimumLength || normalized.Length > maximumLength)
            {
                throw new DomainValidationException(
                    parameterName + " must contain between " + minimumLength + " and " + maximumLength + " characters.");
            }

            for (var index = 0; index < normalized.Length; index++)
            {
                if (char.IsWhiteSpace(normalized[index]))
                {
                    throw new DomainValidationException(parameterName + " cannot contain whitespace.");
                }
            }

            return normalized;
        }

        private void EnsureActive()
        {
            if (Status != UserSessionStatus.Active)
            {
                throw new DomainValidationException("Only active sessions can be changed.");
            }
        }
    }
}
