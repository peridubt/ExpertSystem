using ExpertSystem.RuleEngine.Core.Domain;

namespace ExpertSystem.RuleEngine.Core.Rules
{
    public static class DecisionMutations
    {
        public static int Suggest(GameDecision decision, TacticalAction action, int score, string reason, string ruleName)
        {
            decision.AddTriggeredRule(ruleName);
            if (score <= decision.UtilityScore) return decision.UtilityScore;

            decision.UtilityScore = score;
            decision.SuggestedAction = action;
            decision.TacticalReason = reason;
            return decision.UtilityScore;
        }

        public static DecisionStatus SetStatus(GameDecision decision, DecisionStatus status, string ruleName)
        {
            decision.AddTriggeredRule(ruleName);
            return decision.Status = status;
        }
    }
}
