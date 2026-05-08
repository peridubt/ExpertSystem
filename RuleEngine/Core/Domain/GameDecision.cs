using System.Collections.Generic;

namespace ExpertSystem.RuleEngine.Core.Domain
{
    public enum DecisionStatus
    {
        Open = 0,
        Execute = 1,
        Review = 2,
        Abort = 3,
    }

    public enum TacticalAction
    {
        Hold = 0,
        Attack = 1,
        Burst = 2,
        HealSelf = 3,
        Interrupt = 4,
        Retreat = 5,
        Defend = 6,
        Taunt = 7,
        HealAlly = 8,
    }

    public class GameDecision
    {
        public string DecisionId { get; set; } = string.Empty;
        public string ActorId { get; set; } = string.Empty;
        public string TargetId { get; set; } = string.Empty;
        public TacticalAction SuggestedAction { get; set; } = TacticalAction.Hold;
        public int UtilityScore { get; set; }
        public string TacticalReason { get; set; } = string.Empty;
        public DecisionStatus Status { get; set; } = DecisionStatus.Open;
        public List<GameAlert> Alerts { get; } = new List<GameAlert>();
        public List<string> TriggeredRules { get; } = new List<string>();

        public void AddTriggeredRule(string ruleName)
        {
            if (string.IsNullOrEmpty(ruleName)) return;
            if (!TriggeredRules.Contains(ruleName))
            {
                TriggeredRules.Add(ruleName);
            }
        }
    }
}
