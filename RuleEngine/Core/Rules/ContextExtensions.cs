using ExpertSystem.RuleEngine.Core.Domain;
using NRules.RuleModel;

namespace ExpertSystem.RuleEngine.Core.Rules
{
    public static class ContextExtensions
    {
        public static void Info(this IContext context, GameDecision decision, string message)
        {
            InsertAlert(context, 1, decision, message);
        }

        public static void Warning(this IContext context, GameDecision decision, string message)
        {
            InsertAlert(context, 2, decision, message);
        }

        public static void Error(this IContext context, GameDecision decision, string message)
        {
            InsertAlert(context, 3, decision, message);
        }

        private static void InsertAlert(IContext context, int severity, GameDecision decision, string message)
        {
            decision.AddTriggeredRule(context.Rule.Name);
            var alert = new GameAlert
            {
                Severity = severity,
                Decision = decision,
                RuleName = context.Rule.Name,
                Message = message,
            };
            context.Insert(alert);
        }
    }
}
