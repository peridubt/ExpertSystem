using ExpertSystem.RuleEngine.Core.Application;
using ExpertSystem.RuleEngine.Core.Domain;

namespace ExpertSystem.RuleEngine.Core.Testing
{
    public class DecisionScenario
    {
        public string Name { get; set; } = string.Empty;
        public CombatSnapshot Snapshot { get; set; } = new CombatSnapshot();
        public DecisionStatus ExpectedStatus { get; set; }
        public TacticalAction ExpectedAction { get; set; }
        public string ExpectedRule { get; set; } = string.Empty;
    }
}
