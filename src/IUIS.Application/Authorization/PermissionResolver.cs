using System;
using System.Collections.Generic;
using System.Linq;

using IUIS.Domain.Identity;

namespace IUIS.Application.Authorization
{
    public sealed class PermissionResolver
    {
        public AuthorizationDecision Resolve(AuthorizationPrincipal principal, AuthorizationRequest request)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (request.AllowedRoles.Count > 0 && !request.AllowedRoles.Contains(principal.PrimaryRole))
                return AuthorizationDecision.Deny("role-incompatible");
            if (principal.ApplicationKind != request.RequiredApplicationKind
                || !IsRoleApplicationCompatible(principal.PrimaryRole, principal.ApplicationKind))
                return AuthorizationDecision.Deny("application-incompatible");
            if (!SessionPurposeAllows(principal.SessionPurpose, request.RequiredPermission))
                return AuthorizationDecision.Deny("session-purpose-restricted");
            if (MatchesAny(principal.DirectRestrictions, request.RequiredPermission))
                return AuthorizationDecision.Deny("direct-restriction");

            var effectivePermissions = BuildEffectivePermissions(principal);
            if (!MatchesAny(effectivePermissions, request.RequiredPermission))
                return AuthorizationDecision.Deny("permission-missing");

            var confidentialityDecision = EvaluateConfidentiality(principal, request, effectivePermissions);
            return confidentialityDecision.IsAllowed ? AuthorizationDecision.Allow() : confidentialityDecision;
        }

        public IReadOnlyCollection<string> ResolveEffectivePermissions(AuthorizationPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            return BuildEffectivePermissions(principal).ToList().AsReadOnly();
        }

        private static HashSet<string> BuildEffectivePermissions(AuthorizationPrincipal principal)
        {
            var effective = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var profile in principal.Profiles.Where(item => item.IsActive))
                foreach (var permission in profile.Permissions) effective.Add(permission);
            foreach (var permission in principal.DirectGrants) effective.Add(permission);
            var restricted = effective.Where(permission => MatchesAny(principal.DirectRestrictions, permission)).ToList();
            foreach (var permission in restricted) effective.Remove(permission);
            return effective;
        }

        private static AuthorizationDecision EvaluateConfidentiality(
            AuthorizationPrincipal principal,
            AuthorizationRequest request,
            ICollection<string> effectivePermissions)
        {
            if (principal.PrimaryRole == PrimaryRole.Student)
            {
                if (request.Confidentiality == ConfidentialityClassification.Public)
                    return AuthorizationDecision.Allow();
                if (request.Confidentiality != ConfidentialityClassification.OwnRecord)
                    return AuthorizationDecision.Deny("student-confidentiality-boundary");
                if (string.IsNullOrWhiteSpace(request.ResourceOwnerPersonRecordId)
                    || !string.Equals(principal.PersonRecordId, request.ResourceOwnerPersonRecordId, StringComparison.Ordinal))
                    return AuthorizationDecision.Deny("record-ownership-mismatch");
                return AuthorizationDecision.Allow();
            }

            if (request.Confidentiality == ConfidentialityClassification.Restricted
                && !MatchesAny(effectivePermissions, "confidentiality.restricted"))
                return AuthorizationDecision.Deny("restricted-confidentiality-permission-missing");
            if (request.Confidentiality == ConfidentialityClassification.HighlyRestricted
                && !MatchesAny(effectivePermissions, "confidentiality.high"))
                return AuthorizationDecision.Deny("high-confidentiality-permission-missing");
            return AuthorizationDecision.Allow();
        }

        private static bool SessionPurposeAllows(SessionPurpose purpose, string requiredPermission)
        {
            if (purpose == SessionPurpose.FullAccess) return true;
            if (purpose == SessionPurpose.FirstLoginPasswordChange)
                return string.Equals(requiredPermission, "security.password.change", StringComparison.OrdinalIgnoreCase);
            if (purpose == SessionPurpose.PasswordResetCompletion)
                return string.Equals(requiredPermission, "security.password.reset.complete", StringComparison.OrdinalIgnoreCase);
            return false;
        }

        private static bool IsRoleApplicationCompatible(PrimaryRole role, SessionApplicationKind applicationKind)
        {
            return role == PrimaryRole.Administrator
                ? applicationKind == SessionApplicationKind.AdministratorApplication
                : applicationKind == SessionApplicationKind.UserApplication;
        }

        private static bool MatchesAny(IEnumerable<string> permissions, string requiredPermission)
        {
            return permissions.Any(permission => PermissionText.Matches(permission, requiredPermission));
        }
    }
}
