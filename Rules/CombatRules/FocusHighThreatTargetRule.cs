using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NRules.Fluent.Dsl;

namespace ExpertSystem.Rules.CombatRules
{
    [Name("Focus high-threat target")]
    public class FocusHighThreatTargetRule : Rule
    {
        public override void Define()
        {
            GameDecision decision = null!;
            CombatState state = null!;

            When()
                .Match(() => decision, d => d.Status == DecisionStatus.Open)
                .Match(() => state,
                    s => s.TargetThreatPercent >= 70 &&
                         s.PlayerHealthPercent > 35 &&
                         s.HasLineOfSight);

            Then()
                .Do(ctx => ctx.Info(decision, "Prioritize high-threat enemy target."))
                .Do(ctx => DecisionMutations.Suggest(decision, TacticalAction.Attack, 60, "Reduce enemy team pressure",
                    ctx.Rule.Name));
        }
    }
}