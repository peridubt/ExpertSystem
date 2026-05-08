using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NRules.Fluent.Dsl;

namespace ExpertSystem.Rules.CombatRules
{
    [Name("Emergency retreat")]
    public class EmergencyRetreatRule : Rule
    {
        public override void Define()
        {
            GameDecision decision = null!;
            CombatState state = null!;

            When()
                .Match(() => decision, d => d.Status == DecisionStatus.Open)
                .Match(() => state,
                    s => s.PlayerHealthPercent <= 25 &&
                         s.NearbyEnemyCount >= 3 &&
                         !s.EscapeSkillReady);

            Then()
                .Do(ctx => ctx.Error(decision, "High risk: retreat recommended."))
                .Do(ctx => DecisionMutations.Suggest(decision, TacticalAction.Retreat, 95, "Critical survivability risk", ctx.Rule.Name));
        }
    }
}
