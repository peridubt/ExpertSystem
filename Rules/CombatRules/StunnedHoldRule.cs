using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NRules.Fluent.Dsl;

namespace ExpertSystem.Rules.CombatRules
{
    [Name("Incapacitated hold")]
    public class StunnedHoldRule : Rule
    {
        public override void Define()
        {
            GameDecision decision = null!;
            CombatState state = null!;

            When()
                .Match(() => decision, d => d.Status == DecisionStatus.Open)
                .Match(() => state, s => s.IsIncapacitated);

            Then()
                .Do(ctx => ctx.Warning(decision, "Character is incapacitated; no active action is possible."))
                .Do(ctx => DecisionMutations.Suggest(decision, TacticalAction.Hold, 100, "Stunned/silenced/rooted — skip turn", ctx.Rule.Name));
        }
    }
}
