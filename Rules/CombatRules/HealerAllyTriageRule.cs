using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NRules.Fluent.Dsl;

namespace ExpertSystem.Rules.CombatRules
{
    [Name("Healer triages ally")]
    public class HealerAllyTriageRule : Rule
    {
        public override void Define()
        {
            GameDecision decision = null!;
            CombatState state = null!;
            CharacterProfile profile = null!;

            When()
                .Match(() => decision, d => d.Status == DecisionStatus.Open)
                .Match(() => profile, p => p.Role == CharacterRole.Healer)
                .Match(() => state,
                    s => !s.IsIncapacitated &&
                         s.LowestAllyHealthPercent <= 30 &&
                         s.PlayerManaPercent >= 15);

            Then()
                .Do(ctx => ctx.Warning(decision, "Ally HP critical — healer should prioritize heal."))
                .Do(ctx => DecisionMutations.Suggest(decision, TacticalAction.HealAlly, 92, "Prevent ally death", ctx.Rule.Name));
        }
    }
}
