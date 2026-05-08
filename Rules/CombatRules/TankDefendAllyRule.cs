using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NRules.Fluent.Dsl;

namespace ExpertSystem.Rules.CombatRules
{
    [Name("Tank defends ally")]
    public class TankDefendAllyRule : Rule
    {
        public override void Define()
        {
            GameDecision decision = null!;
            CombatState state = null!;
            CharacterProfile profile = null!;

            When()
                .Match(() => decision, d => d.Status == DecisionStatus.Open)
                .Match(() => profile, p => p.Role == CharacterRole.Tank)
                .Match(() => state,
                    s => !s.IsIncapacitated &&
                         s.LowestAllyHealthPercent <= 40 &&
                         s.NearbyEnemyCount >= 1 &&
                         s.PlayerHealthPercent >= 30);

            Then()
                .Do(ctx => ctx.Warning(decision, "Ally under pressure — tank should hold aggro and mitigate."))
                .Do(ctx => DecisionMutations.Suggest(decision, TacticalAction.Taunt, 85, "Pull threat off ally and defend", ctx.Rule.Name));
        }
    }
}
