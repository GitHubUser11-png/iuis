using System;

using IUIS.Domain.Identity;

namespace IUIS.Application.Security
{
    public enum AuthenticationStatus
    {
        Unspecified = 0,
        Success = 1,
        InvalidCredentials = 2,
        AccountLocked = 3,
        AccountInactive = 4,
        PasswordChangeRequired = 5,
        EnvironmentDenied = 6
    }

    public sealed class LoginRequest
    {
        public string LoginId { get; set; }
        public string Password { get; set; }
        public SessionApplicationKind ApplicationKind { get; set; }
        public string LocalIpAddress { get; set; }
    }

    public sealed class AuthenticationResult
    {
        public AuthenticationStatus Status { get; set; }
        public UserSession Session { get; set; }
        public EffectiveAccessSnapshot Access { get; set; }
        public string UserMessage { get; set; }
        public DateTime? LockedUntilUtc { get; set; }

        public bool IsAuthenticated
        {
            get
            {
                return Session != null
                    && (Status == AuthenticationStatus.Success
                        || Status == AuthenticationStatus.PasswordChangeRequired);
            }
        }
    }

    public sealed class PasswordChangeRequest
    {
        public SessionCredential Credential { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public sealed class PasswordChangeResult
    {
        public bool Succeeded { get; set; }
        public UserSession Session { get; set; }
        public EffectiveAccessSnapshot Access { get; set; }
        public string UserMessage { get; set; }
    }
}
