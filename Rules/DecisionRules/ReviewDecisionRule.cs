using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NRules.Fluent.Dsl;

namespace ExpertSystem.Rules.DecisionRules
{
    [Name("Review decision")]
    [Priority(-9)]
    public class ReviewDecisionRule : Rule
    {
        public override void Define()
        {
            GameDecision decision = null!;

            When()
                .Match(() => decision, d => d.Status == DecisionStatus.Open)
                .Exists<GameAlert>(a => a.Decision == decision, a => a.Severity == 2);

            Then()
                .Do(ctx => DecisionMutations.SetStatus(decision, DecisionStatus.Review, ctx.Rule.Name))
                .Do(ctx => ctx.Update(decision));
        }
    }
}
