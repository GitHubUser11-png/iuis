using System;
using Microsoft.Extensions.DependencyInjection;

using IUIS.Application.Repositories;
using IUIS.Application.Services.Identity;
using IUIS.Application.Navigation;
using IUIS.Infrastructure.Persistence;
using IUIS.Infrastructure.Security;
using IUIS.Infrastructure.Identity;
using IUIS.Infrastructure.Presentation;
using IUIS.SharedUI.Application;
using IUIS.SharedUI.Navigation;
using IUIS.AdminApp.Configuration;

namespace IUIS.AdminApp.Composition
{
    internal static class AdminAppCompositionRoot
    {
        public static IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            
            var dataRoot = AdminAppSettings.ResolveDataRoot();
            
            // Infrastructure - Core Persistence
            services.AddSingleton<IRepositoryCatalog>(sp => 
                new RepositoryCatalog(dataRoot));
            services.AddSingleton<JsonRepositoryStore>();
            services.AddSingleton<MappedJsonRepository>();
            
            // Infrastructure - Security
            services.AddSingleton<AuthenticationService>();
            services.AddSingleton<PasswordHasher>();
            services.AddSingleton<LoginAttemptService>();
            services.AddSingleton<SessionTokenProtector>();
            services.AddSingleton<IInfrastructure.Security.IAuthenticationService>(sp => sp.GetRequiredService<AuthenticationService>());
            
            // Infrastructure - Identity
            services.AddSingleton<ApplicationIdentifierAllocator>();
            
            // Infrastructure - Presentation
            services.AddSingleton<ApplicationRuntime>();
            
            // Application - Repositories (via Infrastructure)
            services.AddScoped<IUserAccountRepository>(sp =>
                new AggregateRepositoryAdapters.UserAccountAdapter(
                    sp.GetRequiredService<MappedJsonRepository>()));
            services.AddScoped<ISessionRepository>(sp =>
                new AggregateRepositoryAdapters.SessionAdapter(
                    sp.GetRequiredService<MappedJsonRepository>()));
            services.AddScoped<IPermissionProfileRepository>(sp =>
                new AggregateRepositoryAdapters.PermissionProfileAdapter(
                    sp.GetRequiredService<MappedJsonRepository>()));
            
            // Application - Services - Identity & Security
            services.AddScoped<IIdentityQueryService, IdentityQueryServices>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ISecurityPolicyService, SecurityPolicyService>();
            
            // Application - Navigation
            services.AddSingleton<INavigationService, FormNavigationService>();
            
            // SharedUI - Application Runtime Context
            services.AddSingleton(sp => sp.GetRequiredService<ApplicationRuntime>());
            
            return services.BuildServiceProvider();
        }
    }
}
