using ExpertSystem.RuleEngine.Core.Application;
using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using ExpertSystem.RuleEngine.Core.Runtime;
using ExpertSystem.Rules.CombatRules;
using NUnit.Framework;

namespace ExpertSystem.RuleEngine.Tests.Editor
{
    public class RuleEvaluationServiceTests
    {
        private sealed class RecordingNotificationService : INotificationService
        {
            public int AbortCount { get; private set; }
            public string LastDecisionId { get; private set; } = string.Empty;

            public void DecisionAborted(GameDecision decision)
            {
                AbortCount++;
                LastDecisionId = decision.DecisionId;
            }
        }

        [Test]
        public void Evaluate_LethalPressure_ReturnsAbortAndRetreat_AndNotifies()
        {
            var notificationService = new RecordingNotificationService();
            var service = new RuleEvaluationService(new GameRulesEngine(typeof(LowHealthRule).Assembly), notificationService);

            var result = service.Evaluate(new CombatSnapshot
            {
                DecisionId = "T-Abort",
                ActorId = "Player",
                TargetId = "Boss",
                PlayerHealthPercent = 15,
                PlayerManaPercent = 30,
                NearbyEnemyCount = 5,
                StrongEnemyCount = 3,
                EscapeSkillReady = false,
                HasLineOfSight = true,
            });

            Assert.That(result.Status, Is.EqualTo(DecisionStatus.Abort));
            Assert.That(result.SuggestedAction, Is.EqualTo(TacticalAction.Retreat));
            Assert.That(result.TriggeredRules, Does.Contain("Emergency retreat"));
            Assert.That(result.TriggeredRules, Does.Contain("Abort decision"));
            Assert.That(notificationService.AbortCount, Is.EqualTo(1));
            Assert.That(notificationService.LastDecisionId, Is.EqualTo("T-Abort"));
        }

        [Test]
        public void Evaluate_DangerousCast_ReturnsReviewAndInterrupt()
        {
            var service = new RuleEvaluationService(new GameRulesEngine(typeof(LowHealthRule).Assembly), new NullNotificationService());

            var result = service.Evaluate(new CombatSnapshot
            {
                DecisionId = "T-Review",
                ActorId = "Player",
                TargetId = "EnemyMage",
                PlayerHealthPercent = 80,
                PlayerManaPercent = 60,
                NearbyEnemyCount = 1,
                InterruptSkillReady = true,
                TargetIsCastingHighImpactSkill = true,
                HasLineOfSight = true,
            });

            Assert.That(result.Status, Is.EqualTo(DecisionStatus.Review));
            Assert.That(result.SuggestedAction, Is.EqualTo(TacticalAction.Interrupt));
            Assert.That(result.TriggeredRules, Does.Contain("Interrupt dangerous cast"));
            Assert.That(result.TriggeredRules, Does.Contain("Review decision"));
        }

        [Test]
        public void Evaluate_CleanExecuteWindow_ReturnsExecuteAndBurst()
        {
            var service = new RuleEvaluationService(new GameRulesEngine(typeof(LowHealthRule).Assembly), new NullNotificationService());

            var result = service.Evaluate(new CombatSnapshot
            {
                DecisionId = "T-Execute",
                ActorId = "Player",
                TargetId = "EnemyRogue",
                PlayerHealthPercent = 90,
                PlayerManaPercent = 75,
                NearbyEnemyCount = 1,
                TargetHealthPercent = 20,
                HasLineOfSight = true,
            });

            Assert.That(result.Status, Is.EqualTo(DecisionStatus.Execute));
            Assert.That(result.SuggestedAction, Is.EqualTo(TacticalAction.Burst));
            Assert.That(result.TriggeredRules, Does.Contain("Finisher burst"));
            Assert.That(result.TriggeredRules, Does.Contain("Execute decision"));
        }
    }
}
