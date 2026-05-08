using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NRules.Fluent.Dsl;

namespace ExpertSystem.Rules.CombatRules
{
    [Name("Resource pressure")]
    public class ResourcePressureRule : Rule
    {
        public override void Define()
        {
            GameDecision decision = null!;
            CombatState state = null!;

            When()
                .Match(() => decision, d => d.Status == DecisionStatus.Open)
                .Match(() => state, s => s.PlayerManaPercent < 25 || s.SkillOnCooldown);

            Then()
                .Do(ctx => ctx.Warning(decision, "Limited mana or key skill unavailable."));
        }
    }
}
