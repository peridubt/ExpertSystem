using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;

namespace ExpertSystem.RuleEngine.Core.Runtime
{
    public class NullNotificationService : INotificationService
    {
        public void DecisionAborted(GameDecision decision)
        {
            // Intentionally empty. Use this when no side effects are needed.
        }
    }
}
