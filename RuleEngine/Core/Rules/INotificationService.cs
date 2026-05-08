

using ExpertSystem.RuleEngine.Core.Domain;

namespace ExpertSystem.RuleEngine.Core.Rules
{
    public interface INotificationService
    {
        void DecisionAborted(GameDecision decision);
    }
}