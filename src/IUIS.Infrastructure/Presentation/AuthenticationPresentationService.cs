using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using IUIS.Application.Abstractions.Security;
using IUIS.Application.Authorization;
using IUIS.Application.Security;
using IUIS.Domain.Identity;
using IUIS.Infrastructure.Persistence;
using IUIS.Infrastructure.Security;
using AppAuthResult = IUIS.Application.Security.AuthenticationResult;
using InfraAuthResult = IUIS.Infrastructure.Security.AuthenticationResult;

namespace IUIS.Infrastructure.Presentation
{
    public sealed class AuthenticationPresentationService : IAuthenticationPresentationService
    {
        private readonly AuthenticationService _authentication;
        private readonly JsonAuthorizationPrincipalProvider _principalProvider;
        private readonly PermissionResolver _permissionResolver;
        private readonly ProductionRepositoryCatalog _catalog;
        private readonly JsonInfrastructureOptions _options;
        private readonly JsonSerializerOptions _json = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public AuthenticationPresentationService(
            AuthenticationService authentication,
            JsonAuthorizationPrincipalProvider principalProvider,
            PermissionResolver permissionResolver,
            ProductionRepositoryCatalog catalog,
            JsonInfrastructureOptions options)
        {
            _authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));
            _principalProvider = principalProvider ?? throw new ArgumentNullException(nameof(principalProvider));
            _permissionResolver = permissionResolver ?? throw new ArgumentNullException(nameof(permissionResolver));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public AppAuthResult Authenticate(LoginRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var applicationKind = request.ApplicationKind == SessionApplicationKind.AdministratorApplication
                ? "AdministratorApplication"
                : "UserApplication";

            var infrastructureResult = _authentication.Authenticate(
                request.LoginId,
                request.Password,
                applicationKind,
                DateTime.UtcNow);

            if (infrastructureResult.IsLockedOut)
            {
                return new AppAuthResult
                {
                    Status = AuthenticationStatus.AccountLocked,
                    UserMessage = "Your account is temporarily locked. Try again later or use password assistance.",
                    LockedUntilUtc = infrastructureResult.LockedUntilUtc
                };
            }

            if (!infrastructureResult.Succeeded)
            {
                return new AppAuthResult
                {
                    Status = AuthenticationStatus.InvalidCredentials,
                    UserMessage = string.IsNullOrWhiteSpace(infrastructureResult.FailureReason)
                        ? "Invalid Login ID or password."
                        : infrastructureResult.FailureReason
                };
            }

            var account = ReadUserAccount(infrastructureResult.UserId);
            if (account == null)
            {
                return new AppAuthResult
                {
                    Status = AuthenticationStatus.InvalidCredentials,
                    UserMessage = "Invalid Login ID or password."
                };
            }

            PrimaryRole role;
            if (!Enum.TryParse(account.PrimaryRole, true, out role) || role == PrimaryRole.Unspecified)
            {
                return new AppAuthResult
                {
                    Status = AuthenticationStatus.AccountInactive,
                    UserMessage = "This account cannot sign in to the selected application."
                };
            }

            SessionApplicationKind sessionApplicationKind;
            SessionPurpose purpose = infrastructureResult.MustChangePassword
                ? SessionPurpose.FirstLoginPasswordChange
                : SessionPurpose.FullAccess;

            if (!Enum.TryParse(applicationKind, true, out sessionApplicationKind))
                sessionApplicationKind = SessionApplicationKind.UserApplication;

            var session = new UserSession(
                infrastructureResult.SessionId,
                infrastructureResult.SessionToken,
                infrastructureResult.UserId,
                account.LoginId,
                account.PersonRecordId,
                role,
                sessionApplicationKind,
                purpose);

            EffectiveAccessSnapshot access = null;
            if (!infrastructureResult.MustChangePassword)
            {
                var principal = _principalProvider.Load(
                    infrastructureResult.SessionId,
                    infrastructureResult.SessionToken,
                    DateTime.UtcNow);
                access = new EffectiveAccessSnapshot(
                    principal,
                    _permissionResolver.ResolveEffectivePermissions(principal));
            }

            return new AppAuthResult
            {
                Status = infrastructureResult.MustChangePassword
                    ? AuthenticationStatus.PasswordChangeRequired
                    : AuthenticationStatus.Success,
                Session = session,
                Access = access,
                UserMessage = infrastructureResult.MustChangePassword
                    ? "You must change your password before continuing."
                    : "Signed in successfully."
            };
        }

        private PersistedUserAccount ReadUserAccount(string userId)
        {
            var path = _catalog.ResolvePath(_options.DataRoot, "users");
            var envelope = RepositoryEnvelopeJson.Deserialize<PersistedUserAccount>(
                File.ReadAllText(path),
                _json);
            return envelope.Records.SingleOrDefault(
                item => string.Equals(item.Id, userId, StringComparison.Ordinal));
        }
    }
}
