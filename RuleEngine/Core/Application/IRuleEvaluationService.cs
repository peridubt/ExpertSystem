namespace ExpertSystem.RuleEngine.Core.Application
{
    public interface IRuleEvaluationService
    {
        DecisionResult Evaluate(CombatSnapshot snapshot);
    }
}