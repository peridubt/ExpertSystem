using System;
using NRules.Extensibility;

namespace ExpertSystem.RuleEngine.Core.Runtime
{
    public class RuleDependencyResolver : IDependencyResolver
    {
        private readonly IServiceProvider _services;

        public RuleDependencyResolver(IServiceProvider services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public object Resolve(IResolutionContext context, Type serviceType)
        {
            var instance = _services.GetService(serviceType);
            if (instance == null)
            {
                throw new InvalidOperationException($"Unsupported rule dependency type '{serviceType.FullName}'");
            }
            return instance;
        }
    }

    internal sealed class SingleServiceProvider : IServiceProvider
    {
        private readonly Type _type;
        private readonly object _instance;

        public SingleServiceProvider(Type type, object instance)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        public object GetService(Type serviceType)
        {
            return serviceType == _type ? _instance : null;
        }
    }
}
