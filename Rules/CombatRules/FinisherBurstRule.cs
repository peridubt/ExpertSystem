using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NRules.Fluent.Dsl;

namespace ExpertSystem.Rules.CombatRules
{
    [Name("Finisher burst")]
    public class FinisherBurstRule : Rule
    {
        public override void Define()
        {
            GameDecision decision = null!;
            CombatState state = null!;

            When()
                .Match(() => decision, d => d.Status == DecisionStatus.Open)
                .Match(() => state,
                    s => s.TargetHealthPercent <= 25 &&
                         s.PlayerManaPercent >= 30 &&
                         s.NearbyEnemyCount <= 2 &&
                         s.HasLineOfSight);

            Then()
                .Do(ctx => ctx.Info(decision, "Target is vulnerable. Finish quickly."))
                .Do(ctx => DecisionMutations.Suggest(decision, TacticalAction.Burst, 70, "Secure elimination window", ctx.Rule.Name));
        }
    }
}
