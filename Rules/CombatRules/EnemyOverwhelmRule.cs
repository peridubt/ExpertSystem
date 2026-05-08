using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NRules.Fluent.Dsl;

namespace ExpertSystem.Rules.CombatRules
{
    [Name("Enemy overwhelm")]
    public class EnemyOverwhelmRule : Rule
    {
        public override void Define()
        {
            GameDecision decision = null!;
            CombatState state = null!;

            When()
                .Match(() => decision, d => d.Status == DecisionStatus.Open)
                .Match(() => state, s => s.NearbyEnemyCount >= 4 && s.StrongEnemyCount >= 2);

            Then()
                .Do(ctx => ctx.Error(decision, "Enemy pressure too high for direct engagement."));
        }
    }
}
