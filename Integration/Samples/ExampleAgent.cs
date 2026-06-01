// Самодостаточный пример интеграции. Реализует обе стороны контракта (ощущение и
// действие) через поля инспектора — позволяет увидеть работу системы в Play Mode
// без реальной игры. В режиме эволюции изменённые факты пишутся обратно в эти поля.
using ExpertSystem.RuleEngine.Core.Application;
using ExpertSystem.RuleEngine.Core.Domain;
using UnityEngine;

namespace ExpertSystem.Integration.Samples
{
    /// <summary>Демонстрационный агент: поставщик снимка и потребитель решения в одном компоненте.</summary>
    [AddComponentMenu("Expert System/Example Agent")]
    public class ExampleAgent : MonoBehaviour, ICombatSnapshotProvider, IDecisionConsumer
    {
        [Header("Состояние актора — попадает в снимок")]
        [Range(0, 100)] public int playerHealthPercent = 100;
        [Range(0, 100)] public int playerManaPercent = 100;
        [Range(0, 100)] public int targetHealthPercent = 100;
        [Min(0)] public int nearbyEnemyCount;
        public bool hasHealingPotion;
        public bool isStunned;
        public bool interruptSkillReady;
        public bool targetIsCastingHighImpactSkill;

        [Header("Последнее решение — вывод движка (только чтение)")]
        public TacticalAction suggestedAction;
        public DecisionStatus status;
        public int utilityScore;
        public string reason;

        /// <summary>Собирает снимок из полей компонента. Возвращает CombatSnapshot.</summary>
        public CombatSnapshot CaptureSnapshot()
        {
            return new CombatSnapshot
            {
                DecisionId = name,
                ActorId = name,
                PlayerHealthPercent = playerHealthPercent,
                PlayerManaPercent = playerManaPercent,
                TargetHealthPercent = targetHealthPercent,
                NearbyEnemyCount = nearbyEnemyCount,
                HasHealingPotion = hasHealingPotion,
                IsStunned = isStunned,
                InterruptSkillReady = interruptSkillReady,
                TargetIsCastingHighImpactSkill = targetIsCastingHighImpactSkill,
                HasLineOfSight = true,
            };
        }

        /// <summary>Принимает решение и сохраняет его в поля для показа. Принимает результат вывода.</summary>
        public void OnDecision(DecisionResult result)
        {
            suggestedAction = result.SuggestedAction;
            status = result.Status;
            utilityScore = result.UtilityScore;
            reason = result.TacticalReason;
            // Реальная игра здесь перевела бы result.SuggestedAction в конкретное поведение.
        }

        /// <summary>Записывает изменённые правилами факты обратно в поля. Принимает снимок после вывода.</summary>
        public void OnStateEvolved(CombatSnapshot updated)
        {
            playerHealthPercent = updated.PlayerHealthPercent;
            playerManaPercent = updated.PlayerManaPercent;
            targetHealthPercent = updated.TargetHealthPercent;
        }
    }
}
