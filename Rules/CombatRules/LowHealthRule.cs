using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NRules.Fluent.Dsl;

namespace ExpertSystem.Rules.CombatRules
{
    [Name("Low health emergency")]
    public class LowHealthRule : Rule
    {
        public override void Define()
        {
            GameDecision decision = null!;
            CombatState state = null!;

            When()
                .Match(() => decision, d => d.Status == DecisionStatus.Open)
                .Match(() => state, s => s.PlayerHealthPercent <= 20);

            Then()
                .Do(ctx => ctx.Error(decision, "Player health critical. Abort aggressive action."));
        }
    }
}
