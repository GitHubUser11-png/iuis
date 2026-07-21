using System;
using System.Collections.Generic;
using System.Linq;

using IUIS.Application.Authorization;

namespace IUIS.Application.Security
{
    public sealed class EffectiveAccessSnapshot
    {
        private readonly HashSet<string> _permissions;

        public EffectiveAccessSnapshot(
            AuthorizationPrincipal principal,
            IEnumerable<string> effectivePermissions)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            Principal = principal;
            _permissions = new HashSet<string>(
                (effectivePermissions ?? Enumerable.Empty<string>())
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value.Trim().ToLowerInvariant()),
                StringComparer.OrdinalIgnoreCase);
        }

        public AuthorizationPrincipal Principal { get; private set; }

        public bool HasPermission(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
                return true;

            var required = permission.Trim().ToLowerInvariant();
            foreach (var granted in _permissions)
            {
                if (PermissionText.Matches(granted, required))
                    return true;
            }

            return false;
        }
    }
}
