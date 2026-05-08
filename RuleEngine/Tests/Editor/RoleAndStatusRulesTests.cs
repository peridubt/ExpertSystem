using ExpertSystem.RuleEngine.Core.Application;
using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Runtime;
using ExpertSystem.Rules.CombatRules;
using NUnit.Framework;

namespace ExpertSystem.RuleEngine.Tests.Editor
{
    public class RoleAndStatusRulesTests
    {
        private static RuleEvaluationService CreateService()
        {
            return new RuleEvaluationService(new GameRulesEngine(typeof(LowHealthRule).Assembly), new NullNotificationService());
        }

        [Test]
        public void Evaluate_Stunned_ForcesHold()
        {
            var service = CreateService();

            var result = service.Evaluate(new CombatSnapshot
            {
                DecisionId = "T-Stunned",
                ActorId = "Player",
                TargetId = "EnemyMage",
                PlayerHealthPercent = 80,
                PlayerManaPercent = 60,
                NearbyEnemyCount = 1,
                TargetHealthPercent = 20,
                IsStunned = true,
                HasLineOfSight = true,
            });

            Assert.That(result.SuggestedAction, Is.EqualTo(TacticalAction.Hold));
            Assert.That(result.TriggeredRules, Does.Contain("Incapacitated hold"));
        }

        [Test]
        public void Evaluate_TankWithInjuredAlly_SuggestsTaunt()
        {
            var service = CreateService();

            var result = service.Evaluate(new CombatSnapshot
            {
                DecisionId = "T-Tank",
                ActorId = "TankPlayer",
                TargetId = "EnemyBoss",
                PlayerHealthPercent = 75,
                PlayerManaPercent = 40,
                NearbyEnemyCount = 2,
                LowestAllyHealthPercent = 25,
                Role = CharacterRole.Tank,
                HasLineOfSight = true,
            });

            Assert.That(result.SuggestedAction, Is.EqualTo(TacticalAction.Taunt));
            Assert.That(result.TriggeredRules, Does.Contain("Tank defends ally"));
        }

        [Test]
        public void Evaluate_HealerWithCriticalAlly_SuggestsHealAlly()
        {
            var service = CreateService();

            var result = service.Evaluate(new CombatSnapshot
            {
                DecisionId = "T-Healer",
                ActorId = "HealerPlayer",
                TargetId = "EnemyRogue",
                PlayerHealthPercent = 80,
                PlayerManaPercent = 70,
                NearbyEnemyCount = 1,
                LowestAllyHealthPercent = 20,
                Role = CharacterRole.Healer,
                HasLineOfSight = true,
            });

            Assert.That(result.SuggestedAction, Is.EqualTo(TacticalAction.HealAlly));
            Assert.That(result.TriggeredRules, Does.Contain("Healer triages ally"));
        }

        [Test]
        public void Evaluate_MultipleEnemyCasters_SuggestsAoeInterrupt()
        {
            var service = CreateService();

            var snapshot = new CombatSnapshot
            {
                DecisionId = "T-MultiCast",
                ActorId = "Player",
                TargetId = "EnemyMage1",
                PlayerHealthPercent = 85,
                PlayerManaPercent = 60,
                NearbyEnemyCount = 3,
                InterruptSkillReady = true,
                HasLineOfSight = true,
            };

            snapshot.Enemies.Add(new EnemyProfile
            {
                EnemyId = "EnemyMage1",
                Archetype = EnemyArchetype.Caster,
                HealthPercent = 90,
                IsCasting = true,
            });
            snapshot.Enemies.Add(new EnemyProfile
            {
                EnemyId = "EnemyMage2",
                Archetype = EnemyArchetype.Caster,
                HealthPercent = 80,
                IsCasting = true,
            });
            snapshot.Enemies.Add(new EnemyProfile
            {
                EnemyId = "EnemyMelee",
                Archetype = EnemyArchetype.Melee,
                HealthPercent = 100,
                IsCasting = false,
            });

            var result = service.Evaluate(snapshot);

            Assert.That(result.SuggestedAction, Is.EqualTo(TacticalAction.Interrupt));
            Assert.That(result.TriggeredRules, Does.Contain("Multi-caster AOE interrupt"));
        }
    }
}
