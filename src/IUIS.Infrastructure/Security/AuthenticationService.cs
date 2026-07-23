using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using IUIS.Infrastructure.Persistence;

namespace IUIS.Infrastructure.Security
{
    public sealed class AuthenticationService
    {
        private readonly ProductionRepositoryCatalog _catalog;
        private readonly JsonInfrastructureOptions _options;
        private readonly CentralIdSequenceService _ids;
        private readonly LoginAttemptService _attempts;
        private readonly JournaledTransactionCoordinator _transactions;
        private readonly PasswordHasher _passwords = new PasswordHasher();
        private readonly SessionTokenProtector _tokens = new SessionTokenProtector();
        private readonly AtomicFileWriter _writer = new AtomicFileWriter();
        private readonly JsonSerializerOptions _json = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public AuthenticationService(
            ProductionRepositoryCatalog catalog,
            JsonInfrastructureOptions options)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _ids = new CentralIdSequenceService(catalog, options);
            _attempts = new LoginAttemptService(catalog, options);
            _transactions = new JournaledTransactionCoordinator(catalog, options);
        }

        public AuthenticationResult Authenticate(
            string loginId,
            string password,
            string applicationKind,
            DateTime utcNow)
        {
            RequireUtc(utcNow);
            var normalized = string.IsNullOrWhiteSpace(loginId)
                ? string.Empty
                : loginId.Trim().ToLowerInvariant();
            var policy = ReadPolicy();
            var lockout = _attempts.Evaluate(normalized, utcNow, policy);
            if (lockout.IsLockedOut)
            {
                return new AuthenticationResult
                {
                    IsLockedOut = true,
                    LockedUntilUtc = lockout.LockedUntilUtc,
                    FailureReason = "Account is temporarily locked."
                };
            }

            var account = ReadUsers().Records.SingleOrDefault(
                item => string.Equals(item.LoginId, normalized, StringComparison.Ordinal));
            var attemptId = _ids.Allocate(
                "LAT",
                utcNow.Year,
                account == null ? "SYSTEM" : account.Id);
            if (account == null
                || !string.Equals(account.Status, "Active", StringComparison.OrdinalIgnoreCase)
                || !_passwords.Verify(password, account.CredentialHash))
            {
                _attempts.RecordFailure(
                    attemptId,
                    normalized,
                    account == null ? null : account.Id,
                    "Invalid credentials.",
                    applicationKind,
                    utcNow);
                var after = _attempts.Evaluate(normalized, utcNow, policy);
                return new AuthenticationResult
                {
                    Succeeded = false,
                    IsLockedOut = after.IsLockedOut,
                    LockedUntilUtc = after.LockedUntilUtc,
                    FailureReason = after.IsLockedOut
                        ? "Account is temporarily locked."
                        : "Invalid credentials."
                };
            }

            _attempts.RecordSuccess(
                attemptId,
                normalized,
                account.Id,
                applicationKind,
                utcNow);
            string rawToken;
            var session = CreateSession(
                account,
                applicationKind,
                utcNow,
                account.MustChangePassword
                    ? "FirstLoginPasswordChange"
                    : "FullAccess",
                out rawToken);
            AppendSession(session);
            return new AuthenticationResult
            {
                Succeeded = true,
                UserId = account.Id,
                SessionId = session.Id,
                SessionToken = rawToken,
                MustChangePassword = account.MustChangePassword
            };
        }

        public AuthenticationResult CompleteForcedPasswordChange(
            string userId,
            string restrictedSessionId,
            string newPassword,
            DateTime utcNow)
        {
            RequireUtc(utcNow);
            var policy = ReadPolicy();
            if (string.IsNullOrEmpty(newPassword)
                || newPassword.Length < policy.MinimumPasswordLength)
                throw new ArgumentException(
                    "The new password does not meet the minimum length.",
                    nameof(newPassword));

            var users = ReadUsers();
            var sessions = ReadSessions();
            var account = users.Records.SingleOrDefault(
                item => string.Equals(item.Id, userId, StringComparison.Ordinal));
            var restricted = sessions.Records.SingleOrDefault(
                item => string.Equals(item.Id, restrictedSessionId, StringComparison.Ordinal));
            if (account == null
                || restricted == null
                || !account.MustChangePassword
                || restricted.Status != "Active"
                || restricted.Purpose != "FirstLoginPasswordChange"
                || restricted.UserId != account.Id
                || restricted.TokenDigestVersion != SessionTokenProtector.CurrentDigestVersion
                || string.IsNullOrWhiteSpace(restricted.TokenDigest)
                || utcNow >= restricted.AbsoluteExpiresAtUtc)
                throw new InvalidOperationException(
                    "A valid restricted first-login session is required.");

            account.CredentialHash = _passwords.Hash(
                newPassword,
                policy.Pbkdf2Iterations);
            account.SecurityStamp = NewSecurityStamp();
            account.MustChangePassword = false;
            account.UpdatedAtUtc = utcNow;
            account.Version = checked(account.Version + 1);
            restricted.Status = "Revoked";
            restricted.RevokedAtUtc = utcNow;
            string rawToken;
            var full = CreateSession(
                account,
                restricted.ApplicationKind,
                utcNow,
                "FullAccess",
                out rawToken);
            sessions.Records.Add(full);
            users.Revision = checked(users.Revision + 1);
            users.UpdatedAtUtc = utcNow;
            users.UpdatedByUserId = account.Id;
            sessions.Revision = checked(sessions.Revision + 1);
            sessions.UpdatedAtUtc = utcNow;
            sessions.UpdatedByUserId = account.Id;

            _transactions.Execute(new[]
            {
                new TransactionMutation(
                    "users",
                    RepositoryEnvelopeJson.Serialize(users, _json)),
                new TransactionMutation(
                    "sessions",
                    RepositoryEnvelopeJson.Serialize(sessions, _json))
            });

            return new AuthenticationResult
            {
                Succeeded = true,
                UserId = account.Id,
                SessionId = full.Id,
                SessionToken = rawToken,
                MustChangePassword = false
            };
        }

        private PersistedSessionRecord CreateSession(
            PersistedUserAccount account,
            string applicationKind,
            DateTime utcNow,
            string purpose,
            out string rawToken)
        {
            rawToken = _tokens.IssueRawToken();
            return new PersistedSessionRecord
            {
                Id = _ids.Allocate("SES", utcNow.Year, account.Id),
                UserId = account.Id,
                TokenDigestVersion = SessionTokenProtector.CurrentDigestVersion,
                TokenDigest = _tokens.ComputeDigest(rawToken),
                LegacyTokenHash = null,
                SecurityStampSnapshot = account.SecurityStamp,
                ApplicationKind = applicationKind,
                Purpose = purpose,
                Status = "Active",
                IssuedAtUtc = utcNow,
                LastActivityAtUtc = utcNow,
                InactivityExpiresAtUtc = utcNow.AddMinutes(
                    purpose == "FirstLoginPasswordChange" ? 10 : 15),
                AbsoluteExpiresAtUtc = utcNow.AddHours(
                    purpose == "FirstLoginPasswordChange" ? 1 : 8)
            };
        }

        private void AppendSession(PersistedSessionRecord session)
        {
            var path = _catalog.ResolvePath(_options.DataRoot, "sessions");
            using (CrossProcessFileLock.Acquire(path, _options.LockTimeout))
            {
                var envelope = ReadSessionsUnlocked(path);
                envelope.Records.Add(session);
                envelope.Revision = checked(envelope.Revision + 1);
                envelope.UpdatedAtUtc = DateTime.UtcNow;
                envelope.UpdatedByUserId = session.UserId;
                _writer.WriteUtf8(
                    path,
                    RepositoryEnvelopeJson.Serialize(envelope, _json));
            }
        }

        private RepositoryEnvelope<PersistedUserAccount> ReadUsers()
        {
            var path = _catalog.ResolvePath(_options.DataRoot, "users");
            using (CrossProcessFileLock.Acquire(path, _options.LockTimeout))
                return RepositoryEnvelopeJson.Deserialize<PersistedUserAccount>(
                    File.ReadAllText(path),
                    _json);
        }

        private RepositoryEnvelope<PersistedSessionRecord> ReadSessions()
        {
            var path = _catalog.ResolvePath(_options.DataRoot, "sessions");
            using (CrossProcessFileLock.Acquire(path, _options.LockTimeout))
                return ReadSessionsUnlocked(path);
        }

        private RepositoryEnvelope<PersistedSessionRecord> ReadSessionsUnlocked(string path)
        {
            var envelope = RepositoryEnvelopeJson.Deserialize<PersistedSessionRecord>(
                File.ReadAllText(path),
                _json);
            if (envelope == null || envelope.Records == null)
                throw new InvalidDataException("Session repository is invalid.");
            return envelope;
        }

        private SecurityPolicyRecord ReadPolicy()
        {
            var path = _catalog.ResolvePath(_options.DataRoot, "security_policy");
            using (CrossProcessFileLock.Acquire(path, _options.LockTimeout))
            {
                var envelope = RepositoryEnvelopeJson.Deserialize<SecurityPolicyRecord>(
                    File.ReadAllText(path),
                    _json);
                if (envelope == null
                    || envelope.Records == null
                    || envelope.Records.Count != 1)
                    throw new InvalidDataException(
                        "Exactly one security policy is required.");
                return envelope.Records[0];
            }
        }

        private static string NewSecurityStamp()
        {
            return Guid.NewGuid().ToString("N");
        }

        private static void RequireUtc(DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc)
                throw new ArgumentException("UTC timestamp is required.");
        }
    }
}
