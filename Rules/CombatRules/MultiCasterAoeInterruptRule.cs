using System.Collections.Generic;
using System.Linq;
using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NRules.Fluent.Dsl;

namespace ExpertSystem.Rules.CombatRules
{
    [Name("Multi-caster AOE interrupt")]
    public class MultiCasterAoeInterruptRule : Rule
    {
        public override void Define()
        {
            GameDecision decision = null!;
            CombatState state = null!;
            IEnumerable<EnemyProfile> casters = null!;

            When()
                .Match(() => decision, d => d.Status == DecisionStatus.Open)
                .Match(() => state, s => !s.IsIncapacitated && s.InterruptSkillReady)
                .Query(() => casters, q => q
                    .Match<EnemyProfile>(e => e.IsCasting)
                    .Collect())
                .Having(() => casters.Count() >= 2);

            Then()
                .Do(ctx => ctx.Error(decision, "Multiple enemies casting simultaneously — AOE interrupt required."))
                .Do(ctx => DecisionMutations.Suggest(decision, TacticalAction.Interrupt, 95, "Prevent multi-target burst", ctx.Rule.Name));
        }
    }
}
