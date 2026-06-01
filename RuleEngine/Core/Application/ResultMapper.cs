// Преобразует доменное решение GameDecision в плоский DTO DecisionResult,
// который отдаётся наружу (инкапсуляция доменной модели от потребителя).
using ExpertSystem.RuleEngine.Core.Domain;

namespace ExpertSystem.RuleEngine.Core.Application
{
    /// <summary>Маппер доменного решения в выходной DTO.</summary>
    public static class ResultMapper
    {
        /// <summary>
        /// Копирует поля решения и преобразует списки оповещений и сработавших правил.
        /// Принимает доменное решение, возвращает DecisionResult.
        /// </summary>
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
