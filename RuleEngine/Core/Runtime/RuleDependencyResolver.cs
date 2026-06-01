// Связывает правила NRules с внешними сервисами: когда правило запрашивает
// зависимость (например, сервис уведомлений), резолвер берёт её из IServiceProvider.
using System;
using NRules.Extensibility;

namespace ExpertSystem.RuleEngine.Core.Runtime
{
    /// <summary>
    /// Реализация механизма внедрения зависимостей NRules поверх стандартного
    /// IServiceProvider.
    /// </summary>
    public class RuleDependencyResolver : IDependencyResolver
    {
        private readonly IServiceProvider _services;

        /// <summary>Принимает провайдер служб, из которого будут браться зависимости.</summary>
        public RuleDependencyResolver(IServiceProvider services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>Возвращает экземпляр запрошенного правилом типа; бросает исключение,
        /// если служба не зарегистрирована.</summary>
        public object Resolve(IResolutionContext context, Type serviceType)
        {
            var instance = _services.GetService(serviceType);
            if (instance == null)
            {
                throw new InvalidOperationException($"Неподдерживаемый тип зависимости правила '{serviceType.FullName}'");
            }
            return instance;
        }
    }

    /// <summary>Провайдер на одну службу: отдаёт экземпляр только для своего типа.</summary>
    internal sealed class SingleServiceProvider : IServiceProvider
    {
        private readonly Type _type;
        private readonly object _instance;

        /// <summary>Запоминает тип службы и её единственный экземпляр.</summary>
        public SingleServiceProvider(Type type, object instance)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        /// <summary>Возвращает экземпляр, если тип совпал, иначе null.</summary>
        public object GetService(Type serviceType)
        {
            return serviceType == _type ? _instance : null;
        }
    }
}
