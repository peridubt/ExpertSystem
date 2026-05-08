using System;
using System.Collections.Generic;
using System.Reflection;
using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NRules;
using NRules.Fluent;

namespace ExpertSystem.RuleEngine.Core.Runtime
{
    public class GameRulesEngine
    {
        private static readonly Lazy<ISessionFactory> SharedFactory =
            new Lazy<ISessionFactory>(BuildDefaultFactory);

        private readonly ISessionFactory _sessionFactory;

        public GameRulesEngine() : this(SharedFactory.Value)
        {
        }

        public GameRulesEngine(params Assembly[] ruleAssemblies)
            : this(BuildFactory(ruleAssemblies))
        {
        }

        public GameRulesEngine(ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
        }

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

            session.Fire();

            foreach (var alert in session.Query<GameAlert>())
            {
                decision.Alerts.Add(alert);
            }

            return decision;
        }

        public GameDecision Evaluate(GameDecision decision, CombatState combatState, IServiceProvider services)
        {
            return Evaluate(decision, combatState, null, null, services);
        }

        public GameDecision Evaluate(GameDecision decision, CombatState combatState,
            INotificationService notificationService)
        {
            return Evaluate(decision, combatState, null, null,
                new SingleServiceProvider(typeof(INotificationService), notificationService));
        }

        private static ISessionFactory BuildDefaultFactory()
        {
            var repository = new RuleRepository();
            repository.Load(load => load.From(typeof(GameRulesEngine).Assembly));
            return repository.Compile();
        }

        private static ISessionFactory BuildFactory(Assembly[] ruleAssemblies)
        {
            if (ruleAssemblies == null || ruleAssemblies.Length == 0)
                throw new ArgumentException("At least one rule assembly must be provided.", nameof(ruleAssemblies));

            var repository = new RuleRepository();
            repository.Load(load => load.From(ruleAssemblies));
            return repository.Compile();
        }
    }
}