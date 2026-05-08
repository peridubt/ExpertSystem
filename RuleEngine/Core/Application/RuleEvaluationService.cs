using System;
using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using ExpertSystem.RuleEngine.Core.Runtime;

namespace ExpertSystem.RuleEngine.Core.Application
{
    public class RuleEvaluationService : IRuleEvaluationService
    {
        private readonly GameRulesEngine _engine;
        private readonly IServiceProvider _services;

        public RuleEvaluationService(GameRulesEngine engine, INotificationService notificationService)
            : this(engine, new NotificationOnlyServiceProvider(notificationService))
        {
        }

        public RuleEvaluationService(GameRulesEngine engine, IServiceProvider services)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public DecisionResult Evaluate(CombatSnapshot snapshot)
        {
            var decision = new GameDecision
            {
                DecisionId = snapshot.DecisionId,
                ActorId = snapshot.ActorId,
                TargetId = snapshot.TargetId,
            };

            var state = new CombatState
            {
                PlayerHealthPercent = snapshot.PlayerHealthPercent,
                PlayerManaPercent = snapshot.PlayerManaPercent,
                NearbyAllyCount = snapshot.NearbyAllyCount,
                NearbyEnemyCount = snapshot.NearbyEnemyCount,
                StrongEnemyCount = snapshot.StrongEnemyCount,
                LowestAllyHealthPercent = snapshot.LowestAllyHealthPercent,
                SkillOnCooldown = snapshot.SkillOnCooldown,
                HasHealingPotion = snapshot.HasHealingPotion,
                InterruptSkillReady = snapshot.InterruptSkillReady,
                EscapeSkillReady = snapshot.EscapeSkillReady,
                TargetIsBoss = snapshot.TargetIsBoss,
                TargetIsCastingHighImpactSkill = snapshot.TargetIsCastingHighImpactSkill,
                TargetHealthPercent = snapshot.TargetHealthPercent,
                TargetThreatPercent = snapshot.TargetThreatPercent,
                HasLineOfSight = snapshot.HasLineOfSight,
                IsStunned = snapshot.IsStunned,
                IsSilenced = snapshot.IsSilenced,
                IsRooted = snapshot.IsRooted,
                BurstCooldownSeconds = snapshot.BurstCooldownSeconds,
                InterruptCooldownSeconds = snapshot.InterruptCooldownSeconds,
                EscapeCooldownSeconds = snapshot.EscapeCooldownSeconds,
            };

            CharacterProfile profile = null;
            if (snapshot.Role != CharacterRole.Unspecified)
            {
                profile = new CharacterProfile
                {
                    ActorId = snapshot.ActorId,
                    Role = snapshot.Role,
                    RiskTolerance = snapshot.RiskTolerance,
                    Level = snapshot.ActorLevel,
                };
            }

            var evaluatedDecision = _engine.Evaluate(decision, state, profile, snapshot.Enemies, _services);
            return ResultMapper.Map(evaluatedDecision);
        }

        private sealed class NotificationOnlyServiceProvider : IServiceProvider
        {
            private readonly INotificationService _notificationService;

            public NotificationOnlyServiceProvider(INotificationService notificationService)
            {
                _notificationService =
                    notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            }

            public object GetService(Type serviceType)
            {
                return serviceType == typeof(INotificationService) ? _notificationService : null;
            }
        }
    }
}