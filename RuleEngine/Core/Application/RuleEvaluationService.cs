// Фасад над движком. Принимает плоский снимок боя, строит из него доменные факты,
// прогоняет через движок и возвращает решение, а также (для режима эволюции)
// снимок с изменённым правилами состоянием. Скрывает от игры детали NRules.
using System;
using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using ExpertSystem.RuleEngine.Core.Runtime;

namespace ExpertSystem.RuleEngine.Core.Application
{
    /// <summary>Прикладной сервис вывода: снимок на входе, решение на выходе.</summary>
    public class RuleEvaluationService : IRuleEvaluationService
    {
        private readonly GameRulesEngine _engine;
        private readonly IServiceProvider _services;

        /// <summary>Создаёт сервис с движком и сервисом уведомлений.</summary>
        public RuleEvaluationService(GameRulesEngine engine, INotificationService notificationService)
            : this(engine, new NotificationOnlyServiceProvider(notificationService))
        {
        }

        /// <summary>Создаёт сервис с движком и произвольным провайдером служб для правил.</summary>
        public RuleEvaluationService(GameRulesEngine engine, IServiceProvider services)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>Прогоняет снимок и возвращает только рекомендацию (режим советника).</summary>
        public DecisionResult Evaluate(CombatSnapshot snapshot)
        {
            return EvaluateDetailed(snapshot).Decision;
        }

        /// <summary>
        /// Прогоняет снимок и возвращает и рекомендацию, и изменённое состояние.
        /// Принимает снимок ситуации, возвращает EvaluationOutcome.
        /// </summary>
        public EvaluationOutcome EvaluateDetailed(CombatSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

            var decision = new GameDecision
            {
                DecisionId = snapshot.DecisionId,
                ActorId = snapshot.ActorId,
                TargetId = snapshot.TargetId,
            };

            var state = BuildState(snapshot);

            // Профиль создаём только если задана роль — иначе ролевые правила не нужны.
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

            // Движок держит эти ссылки в рабочей памяти; эффект-правила могут изменить
            // state, profile и объекты врагов прямо на месте. После вызова ссылки это отражают.
            var evaluatedDecision = _engine.Evaluate(decision, state, profile, snapshot.Enemies, _services);

            return new EvaluationOutcome
            {
                Decision = ResultMapper.Map(evaluatedDecision),
                UpdatedSnapshot = BuildUpdatedSnapshot(snapshot, state, profile),
            };
        }

        /// <summary>Строит доменный факт состояния боя из снимка.</summary>
        private static CombatState BuildState(CombatSnapshot snapshot)
        {
            return new CombatState
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
        }

        /// <summary>
        /// Собирает новый снимок из изменённого правилами состояния. Принимает исходный
        /// снимок, мутированный факт состояния и профиль. Возвращает снимок "после вывода".
        /// </summary>
        private static CombatSnapshot BuildUpdatedSnapshot(
            CombatSnapshot original, CombatState state, CharacterProfile profile)
        {
            var updated = new CombatSnapshot
            {
                DecisionId = original.DecisionId,
                ActorId = original.ActorId,
                TargetId = original.TargetId,
                PlayerHealthPercent = state.PlayerHealthPercent,
                PlayerManaPercent = state.PlayerManaPercent,
                NearbyAllyCount = state.NearbyAllyCount,
                NearbyEnemyCount = state.NearbyEnemyCount,
                StrongEnemyCount = state.StrongEnemyCount,
                LowestAllyHealthPercent = state.LowestAllyHealthPercent,
                SkillOnCooldown = state.SkillOnCooldown,
                HasHealingPotion = state.HasHealingPotion,
                InterruptSkillReady = state.InterruptSkillReady,
                EscapeSkillReady = state.EscapeSkillReady,
                TargetIsBoss = state.TargetIsBoss,
                TargetIsCastingHighImpactSkill = state.TargetIsCastingHighImpactSkill,
                TargetHealthPercent = state.TargetHealthPercent,
                TargetThreatPercent = state.TargetThreatPercent,
                HasLineOfSight = state.HasLineOfSight,
                IsStunned = state.IsStunned,
                IsSilenced = state.IsSilenced,
                IsRooted = state.IsRooted,
                BurstCooldownSeconds = state.BurstCooldownSeconds,
                InterruptCooldownSeconds = state.InterruptCooldownSeconds,
                EscapeCooldownSeconds = state.EscapeCooldownSeconds,
            };

            // Профиль либо изменён правилами, либо берём исходный.
            if (profile != null)
            {
                updated.Role = profile.Role;
                updated.RiskTolerance = profile.RiskTolerance;
                updated.ActorLevel = profile.Level;
            }
            else
            {
                updated.Role = original.Role;
                updated.RiskTolerance = original.RiskTolerance;
                updated.ActorLevel = original.ActorLevel;
            }

            // Объекты врагов передавались по ссылке и могли быть изменены на месте.
            if (original.Enemies != null)
            {
                updated.Enemies.AddRange(original.Enemies);
            }

            return updated;
        }

        /// <summary>Провайдер служб на один сервис уведомлений (для правил).</summary>
        private sealed class NotificationOnlyServiceProvider : IServiceProvider
        {
            private readonly INotificationService _notificationService;

            public NotificationOnlyServiceProvider(INotificationService notificationService)
            {
                _notificationService =
                    notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            }

            /// <summary>Отдаёт сервис уведомлений по его типу, иначе null.</summary>
            public object GetService(Type serviceType)
            {
                return serviceType == typeof(INotificationService) ? _notificationService : null;
            }
        }
    }
}
