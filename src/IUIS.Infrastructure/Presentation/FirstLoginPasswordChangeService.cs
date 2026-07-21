using System;

using IUIS.Application.Abstractions.Security;
using IUIS.Application.Authorization;
using IUIS.Application.Security;
using IUIS.Domain.Identity;
using IUIS.Infrastructure.Persistence;
using IUIS.Infrastructure.Security;
using AppUserSession = IUIS.Application.Security.UserSession;

namespace IUIS.Infrastructure.Presentation
{
    public sealed class FirstLoginPasswordChangeService : IFirstLoginPasswordChangeService
    {
        private readonly AuthenticationService _authentication;
        private readonly JsonAuthorizationPrincipalProvider _principalProvider;
        private readonly PermissionResolver _permissionResolver;

        public FirstLoginPasswordChangeService(
            AuthenticationService authentication,
            JsonAuthorizationPrincipalProvider principalProvider,
            PermissionResolver permissionResolver)
        {
            _authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));
            _principalProvider = principalProvider ?? throw new ArgumentNullException(nameof(principalProvider));
            _permissionResolver = permissionResolver ?? throw new ArgumentNullException(nameof(permissionResolver));
        }

        public PasswordChangeResult ChangePassword(PasswordChangeRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (request.Credential == null)
            {
                return new PasswordChangeResult
                {
                    Succeeded = false,
                    UserMessage = "Your session is no longer valid."
                };
            }

            if (string.IsNullOrEmpty(request.NewPassword)
                || request.NewPassword != request.ConfirmPassword)
            {
                return new PasswordChangeResult
                {
                    Succeeded = false,
                    UserMessage = "The new password and confirmation do not match."
                };
            }

            try
            {
                var principal = _principalProvider.Load(
                    request.Credential.SessionId,
                    request.Credential.SessionToken,
                    DateTime.UtcNow);

                var infrastructureResult = _authentication.CompleteForcedPasswordChange(
                    principal.UserId,
                    request.Credential.SessionId,
                    request.NewPassword,
                    DateTime.UtcNow);

                if (!infrastructureResult.Succeeded)
                {
                    return new PasswordChangeResult
                    {
                        Succeeded = false,
                        UserMessage = "The password could not be changed."
                    };
                }

                var updatedPrincipal = _principalProvider.Load(
                    infrastructureResult.SessionId,
                    infrastructureResult.SessionToken,
                    DateTime.UtcNow);

                var session = new AppUserSession(
                    infrastructureResult.SessionId,
                    infrastructureResult.SessionToken,
                    infrastructureResult.UserId,
                    ReadLoginId(updatedPrincipal.UserId),
                    updatedPrincipal.PersonRecordId,
                    updatedPrincipal.PrimaryRole,
                    updatedPrincipal.ApplicationKind,
                    SessionPurpose.FullAccess);

                var access = new EffectiveAccessSnapshot(
                    updatedPrincipal,
                    _permissionResolver.ResolveEffectivePermissions(updatedPrincipal));

                return new PasswordChangeResult
                {
                    Succeeded = true,
                    Session = session,
                    Access = access,
                    UserMessage = "Password changed successfully."
                };
            }
            catch (Exception)
            {
                return new PasswordChangeResult
                {
                    Succeeded = false,
                    UserMessage = "The password could not be changed safely."
                };
            }
        }

        private string ReadLoginId(string userId)
        {
            return userId;
        }
    }
}
