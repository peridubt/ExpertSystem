using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NRules.Fluent.Dsl;

namespace ExpertSystem.Rules.CombatRules
{
    [Name("Self heal priority")]
    public class SelfHealRule : Rule
    {
        public override void Define()
        {
            GameDecision decision = null!;
            CombatState state = null!;

            When()
                .Match(() => decision, d => d.Status == DecisionStatus.Open)
                .Match(() => state, s => s.PlayerHealthPercent <= 40 && s.HasHealingPotion);

            Then()
                .Do(ctx => ctx.Warning(decision, "Health low: healing action preferred."))
                .Do(ctx => DecisionMutations.Suggest(decision, TacticalAction.HealSelf, 80, "Restore survivability before offense", ctx.Rule.Name));
        }
    }
}
