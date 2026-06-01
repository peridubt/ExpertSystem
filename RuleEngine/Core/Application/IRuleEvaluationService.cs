// Контракт прикладного сервиса вывода — точка обращения к экспертной системе извне.
namespace ExpertSystem.RuleEngine.Core.Application
{
    /// <summary>Интерфейс сервиса вывода: оценка снимка боя.</summary>
    public interface IRuleEvaluationService
    {
        /// <summary>Прогоняет движок и возвращает только рекомендацию (режим советника).
        /// Принимает снимок, возвращает результат решения.</summary>
        DecisionResult Evaluate(CombatSnapshot snapshot);

        /// <summary>Прогоняет движок и возвращает и рекомендацию, и снимок с изменённым
        /// состоянием (режим советника + эволюция состояния). Принимает снимок,
        /// возвращает EvaluationOutcome.</summary>
        EvaluationOutcome EvaluateDetailed(CombatSnapshot snapshot);
    }
}
