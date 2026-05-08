using System.Collections.Generic;
using ExpertSystem.RuleEngine.Core.Domain;

namespace ExpertSystem.RuleEngine.Core.Application
{
    public class DecisionResult
    {
        public string DecisionId { get; set; } = string.Empty;
        public string ActorId { get; set; } = string.Empty;
        public string TargetId { get; set; } = string.Empty;
        public DecisionStatus Status { get; set; }
        public TacticalAction SuggestedAction { get; set; }
        public int UtilityScore { get; set; }
        public string TacticalReason { get; set; } = string.Empty;
        public List<AlertResult> Alerts { get; } = new List<AlertResult>();
        public List<string> TriggeredRules { get; } = new List<string>();
    }

    public class AlertResult
    {
        public int Severity { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}