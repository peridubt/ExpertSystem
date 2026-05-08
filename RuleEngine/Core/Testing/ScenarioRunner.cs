using System.Collections.Generic;
using ExpertSystem.RuleEngine.Core.Application;

namespace ExpertSystem.RuleEngine.Core.Testing
{
    public class ScenarioRunner
    {
        private readonly IRuleEvaluationService _evaluationService;

        public ScenarioRunner(IRuleEvaluationService evaluationService)
        {
            _evaluationService = evaluationService;
        }

        public IReadOnlyList<ScenarioEvaluation> Run(IEnumerable<DecisionScenario> scenarios)
        {
            var evaluations = new List<ScenarioEvaluation>();
            foreach (var scenario in scenarios)
            {
                var result = _evaluationService.Evaluate(scenario.Snapshot);
                var passed = result.Status == scenario.ExpectedStatus &&
                             result.SuggestedAction == scenario.ExpectedAction &&
                             result.TriggeredRules.Contains(scenario.ExpectedRule);

                evaluations.Add(new ScenarioEvaluation
                {
                    Scenario = scenario,
                    Result = result,
                    Passed = passed,
                    Message = passed ? "Passed" : "Unexpected decision output",
                });
            }

            return evaluations;
        }
    }
}
