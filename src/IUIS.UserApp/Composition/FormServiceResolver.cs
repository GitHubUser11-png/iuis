using System;
using Microsoft.Extensions.DependencyInjection;

namespace IUIS.UserApp.Composition
{
    /// <summary>
    /// Abstraction for resolving services within Forms without exposing the full container.
    /// Prevents Service Locator anti-pattern abuse while enabling DI in WinForms.
    /// </summary>
    public interface IFormServiceResolver
    {
        T Resolve<T>();
        object Resolve(Type serviceType);
        bool TryResolve<T>(out T service);
    }

    internal sealed class ServiceProviderFormResolver : IFormServiceResolver
    {
        private readonly IServiceProvider _provider;

        public ServiceProviderFormResolver(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public T Resolve<T>()
        {
            return _provider.GetRequiredService<T>();
        }

        public object Resolve(Type serviceType)
        {
            return _provider.GetRequiredService(serviceType);
        }

        public bool TryResolve<T>(out T service)
        {
            service = default(T);
            try
            {
                service = _provider.GetService<T>();
                return service != null;
            }
            catch
            {
                return false;
            }
        }
    }
}