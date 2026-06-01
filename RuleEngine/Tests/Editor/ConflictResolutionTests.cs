using ExpertSystem.RuleEngine.Core.Application;
using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Runtime;
using ExpertSystem.Rules.CombatRules;
using NUnit.Framework;

namespace ExpertSystem.RuleEngine.Tests.Editor
{
    /// <summary>
    /// Тесты разрешения конфликтов: когда на одних фактах срабатывают несколько правил,
    /// побеждает действие с наибольшей полезностью, а правила обездвиживания/экстренные
    /// корректно перекрывают более слабые предложения.
    /// </summary>
    [TestFixture]
    public class ConflictResolutionTests
    {
        private static RuleEvaluationService CreateService()
        {
            return new RuleEvaluationService(new GameRulesEngine(typeof(LowHealthRule).Assembly), new NullNotificationService());
        }

        [Test]
        public void EmergencyRetreat_Beats_SelfHeal_When_Both_Match()
        {
            var result = CreateService().Evaluate(new CombatSnapshot
            {
                DecisionId = "conflict-1",
                PlayerHealthPercent = 20,
                NearbyEnemyCount = 4,
                HasHealingPotion = true,
                EscapeSkillReady = false,
                HasLineOfSight = true,
            });

            Assert.That(result.SuggestedAction, Is.EqualTo(TacticalAction.Retreat),
                "EmergencyRetreat (score 95) must outrank SelfHeal (score 80).");
            Assert.That(result.TriggeredRules, Does.Contain("Emergency retreat"));
            Assert.That(result.TriggeredRules, Does.Contain("Self heal priority"),
                "Both rules should have matched and contributed alerts.");
            Assert.That(result.Status, Is.EqualTo(DecisionStatus.Abort));
        }

        [Test]
        public void Stunned_Dominates_FinisherBurst()
        {
            var result = CreateService().Evaluate(new CombatSnapshot
            {
                DecisionId = "conflict-2",
                PlayerHealthPercent = 90,
                PlayerManaPercent = 60,
                TargetHealthPercent = 15,
                NearbyEnemyCount = 1,
                HasLineOfSight = true,
                IsStunned = true,
            });

            Assert.That(result.SuggestedAction, Is.EqualTo(TacticalAction.Hold),
                "Incapacitated hold (score 100) must override Finisher burst (score 70).");
            Assert.That(result.TriggeredRules, Does.Contain("Incapacitated hold"));
        }

        [Test]
        public void Healer_HealingAlly_Beats_FocusHighThreat()
        {
            var result = CreateService().Evaluate(new CombatSnapshot
            {
                DecisionId = "conflict-3",
                PlayerHealthPercent = 80,
                PlayerManaPercent = 70,
                LowestAllyHealthPercent = 25,
                TargetThreatPercent = 80,
                NearbyEnemyCount = 2,
                HasLineOfSight = true,
                Role = CharacterRole.Healer,
            });

            Assert.That(result.SuggestedAction, Is.EqualTo(TacticalAction.HealAlly),
                "Healer triage (score 92) must outrank FocusHighThreat (score 60).");
            Assert.That(result.TriggeredRules, Does.Contain("Healer triages ally"));
            Assert.That(result.TriggeredRules, Does.Contain("Focus high-threat target"),
                "Both rules should match — conflict is resolved by score, not by matching.");
        }

        [Test]
        public void Tank_Taunt_Beats_EmergencyRetreat_When_SelfHealthy_But_AllyLow()
        {
            var result = CreateService().Evaluate(new CombatSnapshot
            {
                DecisionId = "conflict-4",
                PlayerHealthPercent = 70,
                NearbyEnemyCount = 3,
                LowestAllyHealthPercent = 25,
                EscapeSkillReady = false,
                HasLineOfSight = true,
                Role = CharacterRole.Tank,
            });

            Assert.That(result.SuggestedAction, Is.EqualTo(TacticalAction.Taunt),
                "Tank should defend the ally, not retreat, when self HP allows it. " +
                "(EmergencyRetreat requires PlayerHealth <= 25, doesn't fire here.)");
            Assert.That(result.TriggeredRules, Does.Contain("Tank defends ally"));
        }

        [Test]
        public void Review_Status_When_Warning_But_No_Error()
        {
            var result = CreateService().Evaluate(new CombatSnapshot
            {
                DecisionId = "status-review",
                PlayerHealthPercent = 80,
                PlayerManaPercent = 60,
                NearbyEnemyCount = 1,
                InterruptSkillReady = true,
                TargetIsCastingHighImpactSkill = true,
                HasLineOfSight = true,
            });

            Assert.That(result.Status, Is.EqualTo(DecisionStatus.Review),
                "InterruptCaster emits Warning (severity=2) — status goes to Review.");
            Assert.That(result.TriggeredRules, Does.Contain("Review decision"));
            Assert.That(result.TriggeredRules, Does.Not.Contain("Execute decision"));
            Assert.That(result.TriggeredRules, Does.Not.Contain("Abort decision"));
        }

        [Test]
        public void Execute_Status_When_Only_Info_Alerts()
        {
            var result = CreateService().Evaluate(new CombatSnapshot
            {
                DecisionId = "status-exec",
                PlayerHealthPercent = 90,
                PlayerManaPercent = 80,
                NearbyEnemyCount = 1,
                TargetHealthPercent = 20,
                HasLineOfSight = true,
            });

            Assert.That(result.Status, Is.EqualTo(DecisionStatus.Execute),
                "Only Info (severity=1) alerts emitted — status is Execute.");
            Assert.That(result.TriggeredRules, Does.Contain("Execute decision"));
        }

        [Test]
        public void Abort_Precedes_Review_When_Error_Present()
        {
            var result = CreateService().Evaluate(new CombatSnapshot
            {
                DecisionId = "status-abort",
                PlayerHealthPercent = 10,
                NearbyEnemyCount = 5,
                EscapeSkillReady = false,
                InterruptSkillReady = true,
                TargetIsCastingHighImpactSkill = true,
                HasLineOfSight = true,
            });

            Assert.That(result.Status, Is.EqualTo(DecisionStatus.Abort),
                "Error (severity=3) from EmergencyRetreat must override Warning (severity=2) from InterruptCaster.");
            Assert.That(result.TriggeredRules, Does.Contain("Abort decision"));
            Assert.That(result.TriggeredRules, Does.Not.Contain("Review decision"));
            Assert.That(result.TriggeredRules, Does.Not.Contain("Execute decision"));
        }

        [Test]
        public void UtilityScore_Reflects_Winning_Rule()
        {
            var result = CreateService().Evaluate(new CombatSnapshot
            {
                DecisionId = "utility-check",
                PlayerHealthPercent = 20,
                NearbyEnemyCount = 4,
                HasHealingPotion = true,
                EscapeSkillReady = false,
                HasLineOfSight = true,
            });

            Assert.That(result.UtilityScore, Is.EqualTo(95),
                "Winning rule's utility score (EmergencyRetreat=95) must be reflected on the decision.");
        }
    }
}
