using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NRules.Fluent.Dsl;

namespace ExpertSystem.Rules.CombatRules
{
    [Name("Interrupt dangerous cast")]
    public class InterruptCasterRule : Rule
    {
        public override void Define()
        {
            GameDecision decision = null!;
            CombatState state = null!;

            When()
                .Match(() => decision, d => d.Status == DecisionStatus.Open)
                .Match(() => state, s => s.InterruptSkillReady && s.TargetIsCastingHighImpactSkill && s.HasLineOfSight);

            Then()
                .Do(ctx => ctx.Warning(decision, "Enemy high-impact cast detected. Interrupt now."))
                .Do(ctx => DecisionMutations.Suggest(decision, TacticalAction.Interrupt, 90, "Prevent incoming burst or control effect", ctx.Rule.Name));
        }
    }
}
