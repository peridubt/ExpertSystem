// Auto-generated from RuleGraphAsset. Do not edit by hand.
using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NRules.Fluent.Dsl;

namespace ExpertSystem.Rules.Graph.Generated
{
    [Name("Low HP Rule")]
    [Priority(100)]
    public class LowHPRule : Rule
    {
        public override void Define()
        {
            CombatState combatState = null!;
            GameDecision decision = null!;

            When()
                .Match(() => combatState, state => state.PlayerHealthPercent <= 15)
                .Match(() => decision);

            Then()
                .Do(ctx => DecisionMutations.Suggest(decision, TacticalAction.Retreat, 80, "", ctx.Rule.Name));
        }
    }
}
