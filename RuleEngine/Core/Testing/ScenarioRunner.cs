// Прогоняет набор сценариев через сервис вывода и сверяет результат с ожиданиями.
using System.Collections.Generic;
using ExpertSystem.RuleEngine.Core.Application;

namespace ExpertSystem.RuleEngine.Core.Testing
{
    /// <summary>Исполнитель эталонных сценариев.</summary>
    public class ScenarioRunner
    {
        private readonly IRuleEvaluationService _evaluationService;

        /// <summary>Принимает сервис вывода, на котором будут прогоняться сценарии.</summary>
        public ScenarioRunner(IRuleEvaluationService evaluationService)
        {
            _evaluationService = evaluationService;
        }

        /// <summary>
        /// Прогоняет сценарии. Принимает перечень сценариев, возвращает список результатов.
        /// Сценарий считается пройденным при совпадении статуса, действия и наличии
        /// ожидаемого правила среди сработавших.
        /// </summary>
        public IReadOnlyList<ScenarioEvaluation> Run(IEnumerable<DecisionScenario> scenarios)
        {
            var evaluations = new List<ScenarioEvaluation>();
            foreach (var scenario in scenarios)
            {
                var result = _evaluationService.Evaluate(scenario.Snapshot);
                var passed = result.Status == scenario.ExpectedStatus &&
                             result.SuggestedAction == scenario.ExpectedAction &&
                             result.TriggeredRules.Contains(scenario.ExpectedRule);

                evaluations.Add(new ScenarioEvaluation
                {
                    Scenario = scenario,
                    Result = result,
                    Passed = passed,
                    Message = passed ? "Пройден" : "Неожиданный результат решения",
                });
            }

            return evaluations;
        }
    }
}
