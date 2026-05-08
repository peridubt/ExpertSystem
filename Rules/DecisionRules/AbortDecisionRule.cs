using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NRules.Fluent.Dsl;

namespace ExpertSystem.Rules.DecisionRules
{
    [Name("Abort decision")]
    [Priority(-8)]
    public class AbortDecisionRule : Rule
    {
        public override void Define()
        {
            GameDecision decision = null!;

            When()
                .Match(() => decision, d => d.Status == DecisionStatus.Open)
                .Exists<GameAlert>(a => a.Decision == decision, a => a.Severity > 2);

            Then()
                .Do(ctx => DecisionMutations.SetStatus(decision, DecisionStatus.Abort, ctx.Rule.Name))
                .Do(ctx => ctx.Update(decision));
        }
    }
}
