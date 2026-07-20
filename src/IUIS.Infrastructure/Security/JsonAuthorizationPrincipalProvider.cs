using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

using IUIS.Application.Authorization;
using IUIS.Domain.Identity;
using IUIS.Infrastructure.Persistence;

namespace IUIS.Infrastructure.Security
{
    public sealed class JsonAuthorizationPrincipalProvider : IAuthorizationPrincipalProvider
    {
        private readonly ProductionRepositoryCatalog _catalog;
        private readonly JsonInfrastructureOptions _options;
        private readonly JsonSerializerOptions _json;

        public JsonAuthorizationPrincipalProvider(
            ProductionRepositoryCatalog catalog,
            JsonInfrastructureOptions options)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _json = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        }

        public AuthorizationPrincipal Load(string sessionId, string sessionToken, DateTime utcNow)
        {
            RequireUtc(utcNow);
            if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(sessionToken))
                throw InvalidSession();

            RepositoryEnvelope<PersistedUserAccount> users;
            RepositoryEnvelope<PersistedSessionRecord> sessions;
            RepositoryEnvelope<PersistedPermissionProfileRecord> profiles;
            ReadAuthorizationSnapshot(out users, out sessions, out profiles);

            var session = sessions.Records.SingleOrDefault(
                item => string.Equals(item.Id, sessionId.Trim(), StringComparison.Ordinal));
            if (session == null
                || !string.Equals(session.Status, "Active", StringComparison.OrdinalIgnoreCase)
                || utcNow < session.IssuedAtUtc
                || utcNow >= session.InactivityExpiresAtUtc
                || utcNow >= session.AbsoluteExpiresAtUtc
                || !FixedTimeEquals(session.TokenHash, "sha256:" + sessionToken.Trim()))
                throw InvalidSession();

            var account = users.Records.SingleOrDefault(
                item => string.Equals(item.Id, session.UserId, StringComparison.Ordinal));
            if (account == null
                || !string.Equals(account.Status, "Active", StringComparison.OrdinalIgnoreCase)
                || !string.Equals(account.SecurityStamp, session.SecurityStampSnapshot, StringComparison.Ordinal))
                throw InvalidSession();

            PrimaryRole role;
            SessionApplicationKind applicationKind;
            SessionPurpose purpose;
            if (!Enum.TryParse(account.PrimaryRole, true, out role)
                || role == PrimaryRole.Unspecified
                || !Enum.TryParse(session.ApplicationKind, true, out applicationKind)
                || applicationKind == SessionApplicationKind.Unspecified
                || !Enum.TryParse(session.Purpose, true, out purpose)
                || purpose == SessionPurpose.Unspecified
                || !IsRoleApplicationCompatible(role, applicationKind))
                throw InvalidSession();

            var assignedProfileIds = new HashSet<string>(
                account.PermissionProfileIds ?? new List<string>(),
                StringComparer.OrdinalIgnoreCase);
            var assignments = new List<PermissionProfileAssignment>();
            foreach (var profileId in assignedProfileIds)
            {
                var profile = profiles.Records.SingleOrDefault(
                    item => string.Equals(item.Id, profileId, StringComparison.OrdinalIgnoreCase));
                if (profile == null)
                    throw new InvalidDataException(
                        "An assigned permission profile is missing from the authoritative repository.");
                assignments.Add(new PermissionProfileAssignment(
                    profile.Id, profile.IsActive, profile.Permissions ?? new List<string>()));
            }

            return new AuthorizationPrincipal(
                account.Id,
                account.PersonRecordId,
                role,
                applicationKind,
                purpose,
                account.SecurityStamp,
                assignments,
                account.DirectPermissionGrants ?? new List<string>(),
                account.DirectPermissionRestrictions ?? new List<string>());
        }

        private void ReadAuthorizationSnapshot(
            out RepositoryEnvelope<PersistedUserAccount> users,
            out RepositoryEnvelope<PersistedSessionRecord> sessions,
            out RepositoryEnvelope<PersistedPermissionProfileRecord> profiles)
        {
            var names = new[] { "permission_profiles", "sessions", "users" };
            var paths = names.Select(name => _catalog.ResolvePath(_options.DataRoot, name))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToList();
            var locks = new List<CrossProcessFileLock>();
            try
            {
                foreach (var path in paths)
                    locks.Add(CrossProcessFileLock.Acquire(path, _options.LockTimeout));
                users = ReadEnvelope<PersistedUserAccount>("users");
                sessions = ReadEnvelope<PersistedSessionRecord>("sessions");
                profiles = ReadEnvelope<PersistedPermissionProfileRecord>("permission_profiles");
            }
            finally
            {
                for (var index = locks.Count - 1; index >= 0; index--) locks[index].Dispose();
            }
        }

        private RepositoryEnvelope<T> ReadEnvelope<T>(string repositoryName)
        {
            var path = _catalog.ResolvePath(_options.DataRoot, repositoryName);
            if (!File.Exists(path))
                throw new FileNotFoundException("An authorization repository is missing.", path);
            var envelope = JsonSerializer.Deserialize<RepositoryEnvelope<T>>(File.ReadAllText(path), _json);
            if (envelope == null || envelope.Records == null
                || !string.Equals(envelope.Repository, repositoryName, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("An authorization repository envelope is invalid.");
            return envelope;
        }

        private static bool IsRoleApplicationCompatible(PrimaryRole role, SessionApplicationKind applicationKind)
        {
            return role == PrimaryRole.Administrator
                ? applicationKind == SessionApplicationKind.AdministratorApplication
                : applicationKind == SessionApplicationKind.UserApplication;
        }

        private static bool FixedTimeEquals(string left, string right)
        {
            if (left == null || right == null) return false;
            var leftBytes = Encoding.UTF8.GetBytes(left);
            var rightBytes = Encoding.UTF8.GetBytes(right);
            var difference = leftBytes.Length ^ rightBytes.Length;
            var length = Math.Max(leftBytes.Length, rightBytes.Length);
            for (var index = 0; index < length; index++)
            {
                var leftByte = index < leftBytes.Length ? leftBytes[index] : (byte)0;
                var rightByte = index < rightBytes.Length ? rightBytes[index] : (byte)0;
                difference |= leftByte ^ rightByte;
            }
            return difference == 0;
        }

        private static InvalidOperationException InvalidSession()
        { return new InvalidOperationException("The session is not valid for this operation."); }

        private static void RequireUtc(DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc)
                throw new ArgumentException("A UTC timestamp is required.", nameof(value));
        }
    }
}
