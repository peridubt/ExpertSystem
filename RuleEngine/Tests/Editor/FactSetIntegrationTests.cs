using System.IO;
using ExpertSystem.RuleEngine.Core.Application;
using ExpertSystem.RuleEngine.Core.Data;
using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Runtime;
using ExpertSystem.Rules.CombatRules;
using NUnit.Framework;

namespace ExpertSystem.RuleEngine.Tests.Editor
{
    /// <summary>
    /// End-to-end integration tests covering the full cycle:
    ///   JSON file in Data/Facts/  →  FactSetData
    ///                              →  CombatSnapshot (via FactSetMapper)
    ///                              →  GameRulesEngine (NRules session)
    ///                              →  DecisionResult
    ///                              →  DecisionResultData (round-trip back to JSON)
    /// Validates that the storage layer, mapping layer, and rule-engine cooperate correctly.
    /// </summary>
    [TestFixture]
    public class FactSetIntegrationTests
    {
        private ExpertSystemDataService _dataService;
        private RuleEvaluationService _ruleService;

        [SetUp]
        public void SetUp()
        {
            _dataService = new ExpertSystemDataService();
            var engine = new GameRulesEngine(typeof(LowHealthRule).Assembly);
            _ruleService = new RuleEvaluationService(engine, new NullNotificationService());
        }

        [Test]
        public void LowHpRetreat_FromJson_TriggersRetreatAndAbortCascade()
        {
            var factSet = _dataService.LoadFactSet("low-hp-retreat");
            Assert.That(factSet, Is.Not.Null,
                "Fact set 'low-hp-retreat' must exist in Data/Facts/.");

            var snapshot = FactSetMapper.ToSnapshot(factSet);
            var result = _ruleService.Evaluate(snapshot);

            Assert.That(result.SuggestedAction, Is.EqualTo(TacticalAction.Retreat),
                "Critical HP + surrounded should produce a Retreat suggestion.");
            Assert.That(result.Status, Is.EqualTo(DecisionStatus.Abort),
                "Error-severity alert should cascade through Abort decision rule.");
            Assert.That(result.TriggeredRules, Is.Not.Empty,
                "At least one rule should fire.");
            Assert.That(result.Alerts, Is.Not.Empty,
                "At least one alert should be raised by the cascade.");
        }

        [Test]
        public void ExecuteWindow_FromJson_TriggersBurstAndExecuteStatus()
        {
            var factSet = _dataService.LoadFactSet("execute-window");
            Assert.That(factSet, Is.Not.Null);

            var snapshot = FactSetMapper.ToSnapshot(factSet);
            var result = _ruleService.Evaluate(snapshot);

            Assert.That(result.SuggestedAction, Is.EqualTo(TacticalAction.Burst),
                "Healthy player + low-HP target should produce a Burst suggestion.");
            Assert.That(result.Status, Is.EqualTo(DecisionStatus.Execute),
                "Only Info-level alerts should leave the decision in Execute status.");
        }

        [Test]
        public void InterruptCast_FromJson_TriggersInterruptAndReviewStatus()
        {
            var factSet = _dataService.LoadFactSet("interrupt-cast");
            Assert.That(factSet, Is.Not.Null);

            var snapshot = FactSetMapper.ToSnapshot(factSet);
            var result = _ruleService.Evaluate(snapshot);

            Assert.That(result.SuggestedAction, Is.EqualTo(TacticalAction.Interrupt),
                "Enemy casting high-impact + interrupt ready should produce Interrupt.");
            Assert.That(result.Status, Is.EqualTo(DecisionStatus.Review),
                "Warning-severity alert should escalate the decision to Review.");
        }

        [Test]
        public void DecisionResult_RoundTripsThroughJson()
        {
            var factSet = _dataService.LoadFactSet("execute-window");
            var snapshot = FactSetMapper.ToSnapshot(factSet);
            var result = _ruleService.Evaluate(snapshot);

            var resultData = FactSetMapper.ToDataResult(result, factSet.factSetId);
            const string testFileName = "test-roundtrip-result";
            _dataService.SaveDecisionResult(resultData, testFileName);

            try
            {
                var loaded = _dataService.LoadDecisionResult(testFileName);
                Assert.That(loaded, Is.Not.Null, "Saved result should be loadable from disk.");
                Assert.That(loaded.suggestedAction, Is.EqualTo(result.SuggestedAction));
                Assert.That(loaded.status, Is.EqualTo(result.Status));
                Assert.That(loaded.utilityScore, Is.EqualTo(result.UtilityScore));
                Assert.That(loaded.sourceFactSetId, Is.EqualTo(factSet.factSetId));
                Assert.That(loaded.triggeredRules.Count, Is.EqualTo(result.TriggeredRules.Count));
            }
            finally
            {
                var path = Path.Combine(_dataService.ResultsRoot, testFileName + ".json");
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Test]
        public void ListFactSetIds_FindsAllDemoFactSets()
        {
            var ids = _dataService.ListFactSetIds();

            Assert.That(ids, Contains.Item("low-hp-retreat"));
            Assert.That(ids, Contains.Item("execute-window"));
            Assert.That(ids, Contains.Item("interrupt-cast"));
        }
    }
}
