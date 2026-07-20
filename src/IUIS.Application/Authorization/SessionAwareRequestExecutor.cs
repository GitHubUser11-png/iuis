using System;

namespace IUIS.Application.Authorization
{
    public sealed class SessionAwareRequestExecutor
    {
        private readonly IAuthorizationPrincipalProvider _principalProvider;
        private readonly PermissionResolver _permissionResolver;

        public SessionAwareRequestExecutor(
            IAuthorizationPrincipalProvider principalProvider,
            PermissionResolver permissionResolver)
        {
            _principalProvider = principalProvider ?? throw new ArgumentNullException(nameof(principalProvider));
            _permissionResolver = permissionResolver ?? throw new ArgumentNullException(nameof(permissionResolver));
        }

        public TResult Query<TResult>(
            string sessionId,
            string sessionToken,
            DateTime utcNow,
            Func<AuthorizationPrincipal, AuthorizationRequest> requestFactory,
            Func<AuthorizationPrincipal, TResult> query)
        {
            if (requestFactory == null) throw new ArgumentNullException(nameof(requestFactory));
            if (query == null) throw new ArgumentNullException(nameof(query));
            var principal = _principalProvider.Load(sessionId, sessionToken, utcNow);
            Authorize(principal, requestFactory(principal));
            return query(principal);
        }

        public void Command(
            string sessionId,
            string sessionToken,
            DateTime utcNow,
            Func<AuthorizationPrincipal, AuthorizationRequest> requestFactory,
            Action<AuthorizationPrincipal> command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            Command<object>(
                sessionId,
                sessionToken,
                utcNow,
                requestFactory,
                principal =>
                {
                    command(principal);
                    return null;
                });
        }

        public TResult Command<TResult>(
            string sessionId,
            string sessionToken,
            DateTime utcNow,
            Func<AuthorizationPrincipal, AuthorizationRequest> requestFactory,
            Func<AuthorizationPrincipal, TResult> command)
        {
            if (requestFactory == null) throw new ArgumentNullException(nameof(requestFactory));
            if (command == null) throw new ArgumentNullException(nameof(command));
            var principal = _principalProvider.Load(sessionId, sessionToken, utcNow);
            Authorize(principal, requestFactory(principal));
            return command(principal);
        }

        private void Authorize(AuthorizationPrincipal principal, AuthorizationRequest request)
        {
            var decision = _permissionResolver.Resolve(principal, request);
            if (!decision.IsAllowed) throw new AuthorizationDeniedException(decision.ReasonCode);
        }
    }
}
