using ExpertSystem.RuleEngine.Core.Domain;

namespace ExpertSystem.RuleEngine.Core.Application
{
    public static class ResultMapper
    {
        public static DecisionResult Map(GameDecision decision)
        {
            var result = new DecisionResult
            {
                DecisionId = decision.DecisionId,
                ActorId = decision.ActorId,
                TargetId = decision.TargetId,
                Status = decision.Status,
                SuggestedAction = decision.SuggestedAction,
                UtilityScore = decision.UtilityScore,
                TacticalReason = decision.TacticalReason,
            };

            foreach (var ruleName in decision.TriggeredRules)
            {
                result.TriggeredRules.Add(ruleName);
            }

            foreach (var alert in decision.Alerts)
            {
                result.Alerts.Add(new AlertResult
                {
                    Severity = alert.Severity,
                    RuleName = alert.RuleName,
                    Message = alert.Message,
                });
            }

            return result;
        }
    }
}