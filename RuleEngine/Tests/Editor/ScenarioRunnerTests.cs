using ExpertSystem.RuleEngine.Core.Application;
using ExpertSystem.RuleEngine.Core.Runtime;
using ExpertSystem.Rules.CombatRules;
using ExpertSystem.RuleEngine.Core.Testing;
using NUnit.Framework;

namespace ExpertSystem.RuleEngine.Tests.Editor
{
    public class ScenarioRunnerTests
    {
        [Test]
        public void Run_DefaultRpgScenarios_AllPass()
        {
            var service = new RuleEvaluationService(new GameRulesEngine(typeof(LowHealthRule).Assembly), new NullNotificationService());
            var runner = new ScenarioRunner(service);

            var evaluations = runner.Run(DefaultRpgScenarios.Create());

            Assert.That(evaluations, Has.Count.EqualTo(3));
            Assert.That(evaluations, Has.All.Matches<ScenarioEvaluation>(e => e.Passed));
        }
    }
}
