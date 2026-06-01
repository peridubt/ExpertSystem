using ExpertSystem.Integration;
using ExpertSystem.RuleEngine.Core.Application;
using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Runtime;
using NUnit.Framework;

namespace ExpertSystem.RuleEngine.Tests.Editor
{
    /// <summary>
    /// Тесты общего кэша движка интеграционного слоя. Проверяют, что RETE-сеть
    /// компилируется один раз и переиспользуется, и что сервис на общем движке выдаёт
    /// корректные решения — то есть игровая точка входа работает.
    /// </summary>
    [TestFixture]
    public class ExpertSystemRuntimeTests
    {
        [SetUp]
        public void SetUp()
        {
            ExpertSystemRuntime.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            ExpertSystemRuntime.Reset();
        }

        [Test]
        public void Engine_IsCachedAndReused()
        {
            var first = ExpertSystemRuntime.Engine;
            var second = ExpertSystemRuntime.Engine;

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.SameAs(first),
                "The engine must be compiled once and reused across accesses.");
        }

        [Test]
        public void SharedEngine_ProducesValidDecision()
        {
            var service = new RuleEvaluationService(
                ExpertSystemRuntime.Engine, new NullNotificationService());

            var result = service.Evaluate(new CombatSnapshot
            {
                DecisionId = "runtime-test",
                PlayerHealthPercent = 15,
                NearbyEnemyCount = 5,
                EscapeSkillReady = false,
                HasLineOfSight = true,
            });

            Assert.That(result.SuggestedAction, Is.EqualTo(TacticalAction.Retreat),
                "The shared engine must evaluate the full rule set just like a directly-built engine.");
            Assert.That(result.Status, Is.EqualTo(DecisionStatus.Abort));
        }

        [Test]
        public void Reset_ForcesRebuild()
        {
            var first = ExpertSystemRuntime.Engine;
            ExpertSystemRuntime.Reset();
            var rebuilt = ExpertSystemRuntime.Engine;

            Assert.That(rebuilt, Is.Not.Null);
            Assert.That(rebuilt, Is.Not.SameAs(first),
                "After Reset the next access must compile a fresh engine.");
        }
    }
}
