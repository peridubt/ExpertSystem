// Результат прогона одного сценария: фактический вывод и признак совпадения с ожиданием.
using ExpertSystem.RuleEngine.Core.Application;

namespace ExpertSystem.RuleEngine.Core.Testing
{
    /// <summary>Итог проверки сценария.</summary>
    public class ScenarioEvaluation
    {
        public DecisionScenario Scenario { get; set; } = new DecisionScenario();
        public DecisionResult Result { get; set; } = new DecisionResult();
        public bool Passed { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
