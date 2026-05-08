using ExpertSystem.RuleEngine.Core.Application;

namespace ExpertSystem.RuleEngine.Core.Testing
{
    public class ScenarioEvaluation
    {
        public DecisionScenario Scenario { get; set; } = new DecisionScenario();
        public DecisionResult Result { get; set; } = new DecisionResult();
        public bool Passed { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
