using System;

namespace IUIS.Infrastructure.Security
{
    public sealed class SecurityPolicyRecord
    {
        public string Id { get; set; }
        public int MaximumFailedAttempts { get; set; }
        public int ObservationWindowMinutes { get; set; }
        public int LockoutMinutes { get; set; }
        public int MinimumPasswordLength { get; set; }
        public int Pbkdf2Iterations { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    public sealed class PersistedUserAccount
    {
        public string Id { get; set; }
        public string LoginId { get; set; }
        public string PrimaryRole { get; set; }
        public string PersonRecordKind { get; set; }
        public string PersonRecordId { get; set; }
        public string CredentialHash { get; set; }
        public string SecurityStamp { get; set; }
        public string Status { get; set; }
        public bool MustChangePassword { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public long Version { get; set; }
    }

    public sealed class PersistedEmployeeRecord
    {
        public string Id { get; set; }
        public string EmployeeNumber { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string EmailAddress { get; set; }
        public string DepartmentId { get; set; }
        public string PositionTitle { get; set; }
        public bool IsFaculty { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public long Version { get; set; }
    }

    public sealed class LoginAttemptRecord
    {
        public string Id { get; set; }
        public string NormalizedLoginId { get; set; }
        public string UserId { get; set; }
        public bool Succeeded { get; set; }
        public string FailureReason { get; set; }
        public DateTime AttemptedAtUtc { get; set; }
        public string ApplicationKind { get; set; }
    }

    public sealed class PersistedSessionRecord
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string TokenHash { get; set; }
        public string SecurityStampSnapshot { get; set; }
        public string ApplicationKind { get; set; }
        public string Purpose { get; set; }
        public string Status { get; set; }
        public DateTime IssuedAtUtc { get; set; }
        public DateTime LastActivityAtUtc { get; set; }
        public DateTime InactivityExpiresAtUtc { get; set; }
        public DateTime AbsoluteExpiresAtUtc { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
    }

    public sealed class AuthenticationResult
    {
        public bool Succeeded { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime? LockedUntilUtc { get; set; }
        public string UserId { get; set; }
        public string SessionId { get; set; }
        public string SessionToken { get; set; }
        public bool MustChangePassword { get; set; }
        public string FailureReason { get; set; }
    }
}
