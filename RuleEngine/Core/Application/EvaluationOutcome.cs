// Полный результат одного цикла вывода: рекомендация и изменённое состояние.
namespace ExpertSystem.RuleEngine.Core.Application
{
    /// <summary>
    /// Несёт оба выхода экспертной системы: Decision — рекомендация (действие, статус,
    /// оповещения); UpdatedSnapshot — снимок с мутациями, сделанными эффект-правилами.
    /// Подача UpdatedSnapshot обратно на вход моделирует эволюцию состояния по тикам.
    /// </summary>
    public class EvaluationOutcome
    {
        /// <summary>Рекомендация, полученная по итогам вывода.</summary>
        public DecisionResult Decision { get; set; }

        /// <summary>Снимок состояния после применения эффект-правил.</summary>
        public CombatSnapshot UpdatedSnapshot { get; set; }
    }
}
