using ExpertSystem.RuleEngine.Core.Domain;
using NRules.Fluent.Dsl;
using NRules.RuleModel;

namespace ExpertSystem.Rules.EffectRules
{
    /// <summary>
    /// Правило-эффект: в отличие от "советующих" правил, оно напрямую изменяет факт в
    /// рабочей памяти. ЕСЛИ персонаж вне боя (врагов рядом нет) и здоровье неполное ТО
    /// восстановить фиксированную порцию здоровья.
    ///
    /// Маркер RegenApplied гарантирует срабатывание не более одного раза за цикл и не
    /// даёт зациклиться (мутация → Update → повторное сопоставление → мутация → ...).
    /// Для постепенной регенерации вызывающий код повторно подаёт изменённый снимок на
    /// вход: каждый цикл восстанавливает одну порцию.
    /// </summary>
    [Name("Out-of-combat regeneration")]
    public class OutOfCombatRegenRule : Rule
    {
        public const int RegenPerCycle = 10;

        /// <summary>Задаёт условия (When) и действия (Then) правила.</summary>
        public override void Define()
        {
            CombatState state = null!;
            GameDecision decision = null!;

            When()
                .Match(() => decision)
                .Match(() => state,
                    s => s.NearbyEnemyCount == 0,
                    s => s.PlayerHealthPercent < 100)
                .Not<RegenApplied>();

            Then()
                .Do(ctx => ctx.Insert(new RegenApplied())) // маркер первым: исключает повторную активацию
                .Do(ctx => Apply(ctx, state, decision))
                .Do(ctx => ctx.Update(state));
        }

        /// <summary>Восстанавливает здоровье и регистрирует правило. Принимает контекст,
        /// факт состояния и решение.</summary>
        private static void Apply(IContext ctx, CombatState state, GameDecision decision)
        {
            state.PlayerHealthPercent += RegenPerCycle; // сеттер ограничит диапазоном [0, 100]
            decision.AddTriggeredRule(ctx.Rule.Name);
        }
    }

    /// <summary>Служебный факт-маркер. Вставляется после регенерации, чтобы правило не
    /// активировалось повторно в том же цикле.</summary>
    public sealed class RegenApplied
    {
    }
}
