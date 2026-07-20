using System;
using System.Collections.Generic;
using System.Linq;

using IUIS.Domain.Identity;

namespace IUIS.Application.Authorization
{
    public enum ConfidentialityClassification
    {
        Public = 0,
        OwnRecord = 1,
        Internal = 2,
        Restricted = 3,
        HighlyRestricted = 4
    }

    public sealed class PermissionProfileAssignment
    {
        private readonly IReadOnlyList<string> _permissions;

        public PermissionProfileAssignment(
            string profileId,
            bool isActive,
            IEnumerable<string> permissions)
        {
            ProfileId = RequiredText(profileId, nameof(profileId));
            IsActive = isActive;
            _permissions = PermissionText.NormalizeDistinct(permissions);
        }

        public string ProfileId { get; private set; }
        public bool IsActive { get; private set; }
        public IReadOnlyList<string> Permissions { get { return _permissions; } }

        private static string RequiredText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("A value is required.", parameterName);
            return value.Trim();
        }
    }

    public sealed class AuthorizationPrincipal
    {
        private readonly IReadOnlyList<PermissionProfileAssignment> _profiles;
        private readonly IReadOnlyList<string> _directGrants;
        private readonly IReadOnlyList<string> _directRestrictions;

        public AuthorizationPrincipal(
            string userId,
            string personRecordId,
            PrimaryRole primaryRole,
            SessionApplicationKind applicationKind,
            SessionPurpose sessionPurpose,
            string securityStamp,
            IEnumerable<PermissionProfileAssignment> profiles,
            IEnumerable<string> directGrants,
            IEnumerable<string> directRestrictions)
        {
            UserId = RequiredIdentifier(userId, nameof(userId));
            PersonRecordId = RequiredIdentifier(personRecordId, nameof(personRecordId));
            PrimaryRole = RequireDefined(primaryRole, nameof(primaryRole));
            ApplicationKind = RequireDefined(applicationKind, nameof(applicationKind));
            SessionPurpose = RequireDefined(sessionPurpose, nameof(sessionPurpose));
            SecurityStamp = RequiredIdentifier(securityStamp, nameof(securityStamp));
            _profiles = (profiles ?? Enumerable.Empty<PermissionProfileAssignment>())
                .Where(item => item != null).ToList().AsReadOnly();
            _directGrants = PermissionText.NormalizeDistinct(directGrants);
            _directRestrictions = PermissionText.NormalizeDistinct(directRestrictions);
        }

        public string UserId { get; private set; }
        public string PersonRecordId { get; private set; }
        public PrimaryRole PrimaryRole { get; private set; }
        public SessionApplicationKind ApplicationKind { get; private set; }
        public SessionPurpose SessionPurpose { get; private set; }
        public string SecurityStamp { get; private set; }
        public IReadOnlyList<PermissionProfileAssignment> Profiles { get { return _profiles; } }
        public IReadOnlyList<string> DirectGrants { get { return _directGrants; } }
        public IReadOnlyList<string> DirectRestrictions { get { return _directRestrictions; } }

        private static string RequiredIdentifier(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("An identifier is required.", parameterName);
            return value.Trim();
        }

        private static T RequireDefined<T>(T value, string parameterName) where T : struct
        {
            if (!Enum.IsDefined(typeof(T), value) || Convert.ToInt32(value) == 0)
                throw new ArgumentException("The value is invalid.", parameterName);
            return value;
        }
    }

    public sealed class AuthorizationRequest
    {
        private readonly IReadOnlyList<PrimaryRole> _allowedRoles;

        public AuthorizationRequest(
            string requiredPermission,
            SessionApplicationKind requiredApplicationKind,
            ConfidentialityClassification confidentiality,
            string resourceOwnerPersonRecordId,
            IEnumerable<PrimaryRole> allowedRoles)
        {
            RequiredPermission = PermissionText.Normalize(requiredPermission);
            RequiredApplicationKind = RequireDefined(requiredApplicationKind, nameof(requiredApplicationKind));
            if (!Enum.IsDefined(typeof(ConfidentialityClassification), confidentiality))
                throw new ArgumentException("The confidentiality classification is invalid.", nameof(confidentiality));
            Confidentiality = confidentiality;
            ResourceOwnerPersonRecordId = string.IsNullOrWhiteSpace(resourceOwnerPersonRecordId)
                ? null : resourceOwnerPersonRecordId.Trim();
            _allowedRoles = (allowedRoles ?? Enumerable.Empty<PrimaryRole>())
                .Where(role => role != PrimaryRole.Unspecified).Distinct().ToList().AsReadOnly();
        }

        public string RequiredPermission { get; private set; }
        public SessionApplicationKind RequiredApplicationKind { get; private set; }
        public ConfidentialityClassification Confidentiality { get; private set; }
        public string ResourceOwnerPersonRecordId { get; private set; }
        public IReadOnlyList<PrimaryRole> AllowedRoles { get { return _allowedRoles; } }

        private static T RequireDefined<T>(T value, string parameterName) where T : struct
        {
            if (!Enum.IsDefined(typeof(T), value) || Convert.ToInt32(value) == 0)
                throw new ArgumentException("The value is invalid.", parameterName);
            return value;
        }
    }

    public sealed class AuthorizationDecision
    {
        private AuthorizationDecision(bool isAllowed, string reasonCode)
        {
            IsAllowed = isAllowed;
            ReasonCode = reasonCode;
        }

        public bool IsAllowed { get; private set; }
        public string ReasonCode { get; private set; }
        public static AuthorizationDecision Allow() { return new AuthorizationDecision(true, "authorized"); }
        public static AuthorizationDecision Deny(string reasonCode)
        { return new AuthorizationDecision(false, reasonCode ?? "denied"); }
    }

    public sealed class AuthorizationDeniedException : InvalidOperationException
    {
        public AuthorizationDeniedException(string reasonCode)
            : base("The requested operation is not authorized.")
        { ReasonCode = reasonCode ?? "denied"; }
        public string ReasonCode { get; private set; }
    }

    public interface IAuthorizationPrincipalProvider
    {
        AuthorizationPrincipal Load(string sessionId, string sessionToken, DateTime utcNow);
    }

    internal static class PermissionText
    {
        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("A permission name is required.", nameof(value));
            var normalized = value.Trim().ToLowerInvariant();
            if (normalized.Length > 200)
                throw new ArgumentException("A permission name is too long.", nameof(value));
            for (var index = 0; index < normalized.Length; index++)
            {
                var character = normalized[index];
                var valid = (character >= 'a' && character <= 'z')
                    || (character >= '0' && character <= '9')
                    || character == '.' || character == '-' || character == '_'
                    || character == '*';
                if (!valid)
                    throw new ArgumentException("A permission name contains unsupported characters.", nameof(value));
            }
            return normalized;
        }

        public static IReadOnlyList<string> NormalizeDistinct(IEnumerable<string> values)
        {
            return (values ?? Enumerable.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(Normalize)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList().AsReadOnly();
        }

        public static bool Matches(string grantedOrRestricted, string required)
        {
            if (string.Equals(grantedOrRestricted, "*", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(grantedOrRestricted, required, StringComparison.OrdinalIgnoreCase)) return true;
            if (!grantedOrRestricted.EndsWith(".*", StringComparison.Ordinal)) return false;
            var prefix = grantedOrRestricted.Substring(0, grantedOrRestricted.Length - 1);
            return required.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }
    }
}
