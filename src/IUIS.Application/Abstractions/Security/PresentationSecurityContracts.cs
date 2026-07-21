using System;

using IUIS.Application.Security;

namespace IUIS.Application.Abstractions.Security
{
    public interface IAuthenticationPresentationService
    {
        AuthenticationResult Authenticate(LoginRequest request);
    }

    public interface ISessionPresentationService
    {
        bool ValidateSession(SessionCredential credential, out UserSession session, out EffectiveAccessSnapshot access, out string reason);

        void Revoke(SessionCredential credential, string reason);

        void Touch(SessionCredential credential);
    }

    public interface IFirstLoginPasswordChangeService
    {
        PasswordChangeResult ChangePassword(PasswordChangeRequest request);
    }

    public interface INetworkContextProvider
    {
        string[] GetActiveIpv4Addresses();
    }

    public interface IStartupReadinessService
    {
        bool IsRepositoryReady(out string statusMessage);

        bool RequiresBootstrap(out string statusMessage);
    }
}
