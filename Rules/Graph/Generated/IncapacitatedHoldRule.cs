// Auto-generated from RuleGraphAsset. Do not edit by hand.

using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NRules.Fluent.Dsl;

namespace ExpertSystem.Rules.Graph.Generated
{
    [Name("Incapacitated Hold")]
    [Priority(90)]
    public class IncapacitatedHoldRule : Rule
    {
        public override void Define()
        {
            CombatState combatState = null!;
            GameDecision decision = null!;

            When()
                .Or(b1 => b1.Match<CombatState>(() => combatState, combatState => combatState.IsStunned == true)
                    .Match<CombatState>(() => combatState, combatState => combatState.IsSilenced == true)
                    .Match<CombatState>(() => combatState, combatState => combatState.IsRooted == true))
                .Match(() => decision);

            Then()
                .Do(ctx => DecisionMutations.Suggest(decision, TacticalAction.Hold, 50, "Player incapacitated",
                    ctx.Rule.Name));
        }
    }
}