using ExpertSystem.RuleEngine.Core.Rules;

namespace ExpertSystem.RuleEngine.Core.Domain
{
    public class GameAlert
    {
        public int Severity { get; set; }
        public GameDecision Decision { get; set; } = null!;
        public string RuleName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
