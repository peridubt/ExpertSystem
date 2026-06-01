// Пустая реализация сервиса уведомлений. Применяется, когда побочные эффекты
// (внешние оповещения) не нужны: в тестах и интерактивных окнах отладки.
using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;

namespace ExpertSystem.RuleEngine.Core.Runtime
{
    /// <summary>Заглушка INotificationService: ничего не делает.</summary>
    public class NullNotificationService : INotificationService
    {
        /// <summary>Обработчик прерывания решения — намеренно пуст.</summary>
        public void DecisionAborted(GameDecision decision)
        {
        }
    }
}
