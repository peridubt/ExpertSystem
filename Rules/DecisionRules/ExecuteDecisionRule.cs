using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NRules.Fluent.Dsl;

namespace ExpertSystem.Rules.DecisionRules
{
    [Name("Execute decision")]
    [Priority(-10)]
    public class ExecuteDecisionRule : Rule
    {
        public override void Define()
        {
            GameDecision decision = null!;

            When()
                .Match(() => decision, d => d.Status == DecisionStatus.Open)
                .Not<GameAlert>(a => a.Decision == decision, a => a.Severity > 1);

            Then()
                .Do(ctx => DecisionMutations.SetStatus(decision, DecisionStatus.Execute, ctx.Rule.Name))
                .Do(ctx => ctx.Update(decision));
        }
    }
}
