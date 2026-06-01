// Контракт внешних побочных эффектов экспертной системы. Игра реализует его, чтобы
// получать оповещения от правил (например, о прерывании решения).
using ExpertSystem.RuleEngine.Core.Domain;

namespace ExpertSystem.RuleEngine.Core.Rules
{
    /// <summary>Сервис уведомлений, внедряемый в правила через механизм зависимостей.</summary>
    public interface INotificationService
    {
        /// <summary>Вызывается, когда решение переведено в статус Abort. Принимает решение.</summary>
        void DecisionAborted(GameDecision decision);
    }
}
