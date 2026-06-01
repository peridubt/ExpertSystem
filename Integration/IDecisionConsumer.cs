// Точка расширения "действие": игра реализует интерфейс, чтобы превращать вывод
// экспертной системы в реальное поведение и (в режиме эволюции) применять изменения фактов.
using ExpertSystem.RuleEngine.Core.Application;

namespace ExpertSystem.Integration
{
    /// <summary>
    /// Потребитель решения. Реализуется на компоненте игры. Может дополнительно
    /// реализовывать INotificationService — тогда мозг передаст ему оповещения от правил.
    /// </summary>
    public interface IDecisionConsumer
    {
        /// <summary>Вызывается после каждого вывода с рекомендацией. Принимает результат
        /// решения; здесь действие переводится в поведение игры.</summary>
        void OnDecision(DecisionResult result);

        /// <summary>Вызывается только в режиме эволюции после OnDecision со снимком,
        /// отражающим мутации фактов. Принимает снимок; здесь изменения пишутся обратно в игру.</summary>
        void OnStateEvolved(CombatSnapshot updatedSnapshot);
    }
}
