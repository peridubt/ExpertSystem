// Описание тестового сценария: входной снимок и ожидаемый результат вывода.
using ExpertSystem.RuleEngine.Core.Application;
using ExpertSystem.RuleEngine.Core.Domain;

namespace ExpertSystem.RuleEngine.Core.Testing
{
    /// <summary>Эталонный сценарий для проверки движка.</summary>
    public class DecisionScenario
    {
        public string Name { get; set; } = string.Empty;
        public CombatSnapshot Snapshot { get; set; } = new CombatSnapshot();
        public DecisionStatus ExpectedStatus { get; set; }
        public TacticalAction ExpectedAction { get; set; }
        public string ExpectedRule { get; set; } = string.Empty;
    }
}
