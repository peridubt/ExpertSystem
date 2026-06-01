using System.IO;
using ExpertSystem.RuleEngine.Core.Application;
using ExpertSystem.RuleEngine.Core.Data;
using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Runtime;
using ExpertSystem.Rules.CombatRules;
using ExpertSystem.Rules.EffectRules;
using NUnit.Framework;

namespace ExpertSystem.RuleEngine.Tests.Editor
{
    /// <summary>
    /// Тесты правил-эффектов (state-mutation): правил, чей Then-блок изменяет входной
    /// факт в рабочей памяти. Проверяют распространение изменения в возвращаемый снимок,
    /// эволюцию по тикам и сохранение изменённого состояния в JSON. Дополняют тесты
    /// советующих правил, которые проверяют только рекомендации и не меняют факты.
    /// </summary>
    [TestFixture]
    public class StateEvolutionTests
    {
        private RuleEvaluationService _service;

        [SetUp]
        public void SetUp()
        {
            var engine = new GameRulesEngine(typeof(LowHealthRule).Assembly);
            _service = new RuleEvaluationService(engine, new NullNotificationService());
        }

        // Чистое состояние "отдых вне боя": нет врагов, полная мана (чтобы молчало
        // правило ресурсов), цель цела (чтобы молчало правило добивания). Реагировать
        // должно только правило регенерации, оставляя решение в статусе Execute.
        private static CombatSnapshot OutOfCombat(int hp)
        {
            return new CombatSnapshot
            {
                DecisionId = "regen-test",
                ActorId = "Player",
                PlayerHealthPercent = hp,
                PlayerManaPercent = 100,
                TargetHealthPercent = 100,
                NearbyEnemyCount = 0,
                HasLineOfSight = true,
            };
        }

        [Test]
        public void Regen_RestoresHealth_OncePerCycle()
        {
            var outcome = _service.EvaluateDetailed(OutOfCombat(50));

            Assert.That(outcome.UpdatedSnapshot.PlayerHealthPercent,
                Is.EqualTo(50 + OutOfCombatRegenRule.RegenPerCycle),
                "One evaluation cycle must restore exactly one regen increment (marker prevents looping to full).");
            Assert.That(outcome.Decision.TriggeredRules, Does.Contain("Out-of-combat regeneration"));
        }

        [Test]
        public void Regen_DoesNotFire_InCombat()
        {
            var snapshot = OutOfCombat(50);
            snapshot.NearbyEnemyCount = 2; // рядом враги → в бою

            var outcome = _service.EvaluateDetailed(snapshot);

            Assert.That(outcome.UpdatedSnapshot.PlayerHealthPercent, Is.EqualTo(50),
                "Health must not regenerate while enemies are nearby.");
            Assert.That(outcome.Decision.TriggeredRules, Does.Not.Contain("Out-of-combat regeneration"));
        }

        [Test]
        public void Regen_DoesNotFire_AtFullHealth()
        {
            var outcome = _service.EvaluateDetailed(OutOfCombat(100));

            Assert.That(outcome.UpdatedSnapshot.PlayerHealthPercent, Is.EqualTo(100));
            Assert.That(outcome.Decision.TriggeredRules, Does.Not.Contain("Out-of-combat regeneration"));
        }

        [Test]
        public void Regen_EvolvesState_OverSuccessiveCycles()
        {
            var snapshot = OutOfCombat(50);

            // Изменённый снимок каждого цикла подаём на вход следующего — модель времени.
            for (int i = 0; i < 3; i++)
            {
                snapshot = _service.EvaluateDetailed(snapshot).UpdatedSnapshot;
            }

            Assert.That(snapshot.PlayerHealthPercent,
                Is.EqualTo(50 + 3 * OutOfCombatRegenRule.RegenPerCycle),
                "Three cycles must accumulate three regen increments.");
        }

        [Test]
        public void Regen_ClampsAtFullHealth_AcrossCycles()
        {
            var snapshot = OutOfCombat(95);

            for (int i = 0; i < 5; i++)
            {
                snapshot = _service.EvaluateDetailed(snapshot).UpdatedSnapshot;
            }

            Assert.That(snapshot.PlayerHealthPercent, Is.EqualTo(100),
                "Health must clamp at 100 and not overshoot across cycles.");
        }

        [Test]
        public void DecisionSupport_StillWorks_AlongsideMutation()
        {
            // Тот же цикл, что меняет состояние, должен дать и корректное решение.
            var outcome = _service.EvaluateDetailed(OutOfCombat(50));

            Assert.That(outcome.Decision.Status, Is.EqualTo(DecisionStatus.Execute),
                "With no high-severity alerts the decision resolves to Execute.");
        }

        [Test]
        public void EvolvedState_PersistsThroughJson()
        {
            var dataService = new ExpertSystemDataService();
            var outcome = _service.EvaluateDetailed(OutOfCombat(50));

            var data = FactSetMapper.ToData(outcome.UpdatedSnapshot, "test-evolved-state");
            dataService.SaveFactSet(data);

            try
            {
                var reloaded = dataService.LoadFactSet("test-evolved-state");
                Assert.That(reloaded, Is.Not.Null, "Saved evolved state must be loadable.");
                Assert.That(reloaded.combatState.playerHealthPercent,
                    Is.EqualTo(50 + OutOfCombatRegenRule.RegenPerCycle),
                    "The mutated health must survive a save/load round-trip.");
            }
            finally
            {
                var path = Path.Combine(dataService.FactsRoot, "test-evolved-state.json");
                if (File.Exists(path)) File.Delete(path);
            }
        }
    }
}
