// Auto-generated from ScriptedRuleAsset. Do not edit by hand.
using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NRules.Fluent.Dsl;

namespace ExpertSystem.Rules.Scripted.Generated
{
    [Name("MultiEnemyAlert")]
    public class MultiEnemyAlertRule : Rule
    {
        public override void Define()
        {
            CombatState state = null!;
            GameDecision decision = null!;

            When()
                .Match(() => decision, d => d.Status == DecisionStatus.Open)
                .Match(() => state)
                .Exists<EnemyProfile>(e => e.IsElite)
                .Exists<EnemyProfile>(e => e.HealthPercent > 50);

            Then()
                .Do(ctx => ctx.Warning(decision, "Multiple healthy elite enemies present"));
        }
    }
}
