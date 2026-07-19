using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using IUIS.Infrastructure.Persistence;

namespace IUIS.Infrastructure.Security
{
    public sealed class LoginLockoutState
    {
        public bool IsLockedOut { get; set; }
        public int RecentFailureCount { get; set; }
        public DateTime? LockedUntilUtc { get; set; }
    }

    public sealed class LoginAttemptService
    {
        private readonly ProductionRepositoryCatalog _catalog;
        private readonly JsonInfrastructureOptions _options;
        private readonly AtomicFileWriter _writer = new AtomicFileWriter();
        private readonly JsonSerializerOptions _json = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public LoginAttemptService(ProductionRepositoryCatalog catalog, JsonInfrastructureOptions options)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public LoginLockoutState Evaluate(string loginId, DateTime utcNow, SecurityPolicyRecord policy)
        {
            if (utcNow.Kind != DateTimeKind.Utc) throw new ArgumentException("UTC timestamp is required.", nameof(utcNow));
            var normalized = Normalize(loginId);
            var path = _catalog.ResolvePath(_options.DataRoot, "login_attempts");
            using (CrossProcessFileLock.Acquire(path, _options.LockTimeout))
            {
                var envelope = Read(path);
                return EvaluateInternal(envelope.Records, normalized, utcNow, policy);
            }
        }

        public void RecordFailure(string attemptId, string loginId, string userId, string reason, string applicationKind, DateTime utcNow)
        {
            Record(new LoginAttemptRecord
            {
                Id = attemptId,
                NormalizedLoginId = Normalize(loginId),
                UserId = userId,
                Succeeded = false,
                FailureReason = reason,
                ApplicationKind = applicationKind,
                AttemptedAtUtc = RequireUtc(utcNow)
            });
        }

        public void RecordSuccess(string attemptId, string loginId, string userId, string applicationKind, DateTime utcNow)
        {
            Record(new LoginAttemptRecord
            {
                Id = attemptId,
                NormalizedLoginId = Normalize(loginId),
                UserId = userId,
                Succeeded = true,
                FailureReason = null,
                ApplicationKind = applicationKind,
                AttemptedAtUtc = RequireUtc(utcNow)
            });
        }

        private void Record(LoginAttemptRecord attempt)
        {
            var path = _catalog.ResolvePath(_options.DataRoot, "login_attempts");
            using (CrossProcessFileLock.Acquire(path, _options.LockTimeout))
            {
                var envelope = Read(path);
                if (envelope.Records.Any(x => string.Equals(x.Id, attempt.Id, StringComparison.Ordinal)))
                    throw new InvalidOperationException("Duplicate login attempt ID.");
                envelope.Records.Add(attempt);
                envelope.Revision = checked(envelope.Revision + 1);
                envelope.UpdatedAtUtc = DateTime.UtcNow;
                _writer.WriteUtf8(path, JsonSerializer.Serialize(envelope, _json));
            }
        }

        private RepositoryEnvelope<LoginAttemptRecord> Read(string path)
        {
            if (!File.Exists(path))
                return new RepositoryEnvelope<LoginAttemptRecord> { Repository = "login_attempts", SchemaVersion = 1, Revision = 0, Records = new List<LoginAttemptRecord>() };
            var envelope = JsonSerializer.Deserialize<RepositoryEnvelope<LoginAttemptRecord>>(File.ReadAllText(path), _json);
            if (envelope == null || envelope.Records == null) throw new InvalidDataException("Login attempt repository is invalid.");
            return envelope;
        }

        private static LoginLockoutState EvaluateInternal(IList<LoginAttemptRecord> attempts, string loginId, DateTime utcNow, SecurityPolicyRecord policy)
        {
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            var windowStart = utcNow.AddMinutes(-policy.ObservationWindowMinutes);
            var recent = attempts.Where(x => string.Equals(x.NormalizedLoginId, loginId, StringComparison.Ordinal)
                && x.AttemptedAtUtc >= windowStart && x.AttemptedAtUtc <= utcNow)
                .OrderByDescending(x => x.AttemptedAtUtc).ToList();

            var lastSuccess = recent.FirstOrDefault(x => x.Succeeded);
            var failures = recent.Where(x => !x.Succeeded && (lastSuccess == null || x.AttemptedAtUtc > lastSuccess.AttemptedAtUtc)).ToList();
            var state = new LoginLockoutState { RecentFailureCount = failures.Count };
            if (failures.Count >= policy.MaximumFailedAttempts)
            {
                var threshold = failures.OrderBy(x => x.AttemptedAtUtc).Skip(policy.MaximumFailedAttempts - 1).First();
                var lockedUntil = threshold.AttemptedAtUtc.AddMinutes(policy.LockoutMinutes);
                if (utcNow < lockedUntil)
                {
                    state.IsLockedOut = true;
                    state.LockedUntilUtc = lockedUntil;
                }
            }
            return state;
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            return value.Trim().ToLowerInvariant();
        }

        private static DateTime RequireUtc(DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc) throw new ArgumentException("UTC timestamp is required.");
            return value;
        }
    }
}
