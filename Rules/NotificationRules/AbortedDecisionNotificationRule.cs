using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NRules.Fluent.Dsl;

namespace ExpertSystem.Rules.NotificationRules
{
    [Name("Notify aborted decision")]
    public class AbortedDecisionNotificationRule : Rule
    {
        public override void Define()
        {
            INotificationService notificationService = null!;
            GameDecision decision = null!;

            Dependency()
                .Resolve(() => notificationService);

            When()
                .Match(() => decision, d => d.Status == DecisionStatus.Abort);

            Then()
                .Do(_ => notificationService.DecisionAborted(decision));
        }
    }
}
