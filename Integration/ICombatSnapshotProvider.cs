// Точка расширения "ощущение": игра реализует интерфейс, чтобы превращать своё
// состояние в снимок боя, понятный экспертной системе.
using ExpertSystem.RuleEngine.Core.Application;

namespace ExpertSystem.Integration
{
    /// <summary>Поставщик снимка боя. Реализуется на любом компоненте игры.</summary>
    public interface ICombatSnapshotProvider
    {
        /// <summary>Строит снимок текущего состояния мира. Возвращает CombatSnapshot
        /// или null, чтобы пропустить цикл вывода.</summary>
        CombatSnapshot CaptureSnapshot();
    }
}
