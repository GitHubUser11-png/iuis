using System;

using IUIS.Application.Abstractions.Security;
using IUIS.Application.Authorization;
using IUIS.Application.Security;
using IUIS.Infrastructure.Composition;
using IUIS.Infrastructure.Persistence;
using IUIS.Infrastructure.Security;

namespace IUIS.Infrastructure.Presentation
{
    public sealed class ApplicationRuntime
    {
        public ApplicationRuntime(string dataRoot)
        {
            if (string.IsNullOrWhiteSpace(dataRoot))
                throw new ArgumentException("Data root is required.", nameof(dataRoot));

            Options = new JsonInfrastructureOptions(dataRoot);
            Catalog = new ProductionRepositoryCatalog();
            Composition = new IuisCompositionRoot(Catalog, Options);
            Authentication = new AuthenticationService(Catalog, Options);
            PermissionResolver = new PermissionResolver();
            AuthenticationPresentation = new AuthenticationPresentationService(
                Authentication,
                (JsonAuthorizationPrincipalProvider)Composition.PrincipalProvider,
                PermissionResolver,
                Catalog,
                Options);
            SessionPresentation = new SessionPresentationService(
                (JsonAuthorizationPrincipalProvider)Composition.PrincipalProvider,
                PermissionResolver,
                Catalog,
                Options);
            PasswordChange = new FirstLoginPasswordChangeService(
                Authentication,
                (JsonAuthorizationPrincipalProvider)Composition.PrincipalProvider,
                PermissionResolver);
            NetworkContext = new NetworkContextProvider();
            StartupReadiness = new StartupReadinessService(Catalog, Options);
            Bootstrapper = new Bootstrap.ProductionBootstrapper(Catalog, Options);
            CurrentUser = new CurrentUserContext();
        }

        public JsonInfrastructureOptions Options { get; private set; }
        public ProductionRepositoryCatalog Catalog { get; private set; }
        public IuisCompositionRoot Composition { get; private set; }
        public AuthenticationService Authentication { get; private set; }
        public PermissionResolver PermissionResolver { get; private set; }
        public IAuthenticationPresentationService AuthenticationPresentation { get; private set; }
        public ISessionPresentationService SessionPresentation { get; private set; }
        public IFirstLoginPasswordChangeService PasswordChange { get; private set; }
        public INetworkContextProvider NetworkContext { get; private set; }
        public IStartupReadinessService StartupReadiness { get; private set; }
        public Bootstrap.ProductionBootstrapper Bootstrapper { get; private set; }
        public CurrentUserContext CurrentUser { get; private set; }
    }
}
