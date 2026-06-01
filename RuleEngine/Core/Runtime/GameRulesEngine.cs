// Обёртка над движком NRules. Создаёт сессию (рабочую память), вставляет в неё
// факты, запускает вывод по алгоритму RETE и собирает результат в объект решения.
using System;
using System.Collections.Generic;
using System.Reflection;
using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NRules;
using NRules.Fluent;

namespace ExpertSystem.RuleEngine.Core.Runtime
{
    /// <summary>
    /// Точка входа в движок вывода. Хранит скомпилированную фабрику сессий и на каждый
    /// вызов Evaluate создаёт изолированную сессию-рабочую память.
    /// </summary>
    public class GameRulesEngine
    {
        // Фабрика по умолчанию: компилируется один раз и лениво (правила из своей сборки).
        private static readonly Lazy<ISessionFactory> SharedFactory =
            new Lazy<ISessionFactory>(BuildDefaultFactory);

        private readonly ISessionFactory _sessionFactory;

        /// <summary>Движок на правилах из собственной сборки (общая ленивая фабрика).</summary>
        public GameRulesEngine() : this(SharedFactory.Value)
        {
        }

        /// <summary>Движок на правилах из указанных сборок. Используется для подключения
        /// сгенерированных правил из сборки Rules.</summary>
        public GameRulesEngine(params Assembly[] ruleAssemblies)
            : this(BuildFactory(ruleAssemblies))
        {
        }

        /// <summary>Движок на готовой фабрике сессий. Применяется в тестах.</summary>
        public GameRulesEngine(ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
        }

        /// <summary>
        /// Основной метод вывода. Принимает решение и факты ситуации, прогоняет их через
        /// RETE-сеть, возвращает то же решение с заполненными по ходу вывода полями.
        /// </summary>
        public GameDecision Evaluate(
            GameDecision decision,
            CombatState combatState,
            CharacterProfile profile,
            IEnumerable<EnemyProfile> enemies,
            IServiceProvider services)
        {
            if (decision == null) throw new ArgumentNullException(nameof(decision));
            if (combatState == null) throw new ArgumentNullException(nameof(combatState));
            if (services == null) throw new ArgumentNullException(nameof(services));

            // Новая сессия = чистая рабочая память на один цикл вывода.
            var session = _sessionFactory.CreateSession();
            session.DependencyResolver = new RuleDependencyResolver(services);

            session.Insert(decision);
            session.Insert(combatState);

            if (profile != null)
            {
                session.Insert(profile);
            }

            if (enemies != null)
            {
                foreach (var enemy in enemies)
                {
                    if (enemy != null) session.Insert(enemy);
                }
            }

            // Запуск алгоритма RETE: сопоставление фактов с правилами и выполнение действий.
            session.Fire();

            // Оповещения, выпущенные правилами, переносим из рабочей памяти в решение.
            foreach (var alert in session.Query<GameAlert>())
            {
                decision.Alerts.Add(alert);
            }

            return decision;
        }

        /// <summary>Упрощённый вызов без профиля и списка врагов.</summary>
        public GameDecision Evaluate(GameDecision decision, CombatState combatState, IServiceProvider services)
        {
            return Evaluate(decision, combatState, null, null, services);
        }

        /// <summary>Упрощённый вызов, когда нужен только сервис уведомлений.</summary>
        public GameDecision Evaluate(GameDecision decision, CombatState combatState,
            INotificationService notificationService)
        {
            return Evaluate(decision, combatState, null, null,
                new SingleServiceProvider(typeof(INotificationService), notificationService));
        }

        /// <summary>Собирает фабрику сессий из правил собственной сборки движка.</summary>
        private static ISessionFactory BuildDefaultFactory()
        {
            var repository = new RuleRepository();
            repository.Load(load => load.From(typeof(GameRulesEngine).Assembly));
            return repository.Compile();
        }

        /// <summary>Собирает фабрику сессий из переданных сборок. Бросает исключение,
        /// если сборки не заданы.</summary>
        private static ISessionFactory BuildFactory(Assembly[] ruleAssemblies)
        {
            if (ruleAssemblies == null || ruleAssemblies.Length == 0)
                throw new ArgumentException("Нужно указать хотя бы одну сборку с правилами.", nameof(ruleAssemblies));

            var repository = new RuleRepository();
            repository.Load(load => load.From(ruleAssemblies));
            return repository.Compile();
        }
    }
}
