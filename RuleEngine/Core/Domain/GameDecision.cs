// Факт решения — накапливает результат вывода: действие, статус, обоснование,
// оповещения и список сработавших правил. Здесь же — перечисления статусов и действий.
using System.Collections.Generic;

namespace ExpertSystem.RuleEngine.Core.Domain
{
    /// <summary>Статус решения по итогам вывода.</summary>
    public enum DecisionStatus
    {
        Open = 0,    // решение ещё формируется
        Execute = 1, // выполнять как предложено
        Review = 2,  // есть предупреждения, требуется проверка
        Abort = 3,   // критическая ситуация, не выполнять
    }

    /// <summary>Тактическое действие, которое может предложить система.</summary>
    public enum TacticalAction
    {
        Hold = 0,
        Attack = 1,
        Burst = 2,
        HealSelf = 3,
        Interrupt = 4,
        Retreat = 5,
        Defend = 6,
        Taunt = 7,
        HealAlly = 8,
    }

    /// <summary>Формируемое решение. Списки доступны только на чтение — меняются методами класса.</summary>
    public class GameDecision
    {
        public string DecisionId { get; set; } = string.Empty;
        public string ActorId { get; set; } = string.Empty;
        public string TargetId { get; set; } = string.Empty;
        public TacticalAction SuggestedAction { get; set; } = TacticalAction.Hold;
        public int UtilityScore { get; set; }
        public string TacticalReason { get; set; } = string.Empty;
        public DecisionStatus Status { get; set; } = DecisionStatus.Open;
        public List<GameAlert> Alerts { get; } = new List<GameAlert>();
        public List<string> TriggeredRules { get; } = new List<string>();

        /// <summary>Регистрирует имя сработавшего правила без дублей. Принимает имя правила.</summary>
        public void AddTriggeredRule(string ruleName)
        {
            if (string.IsNullOrEmpty(ruleName)) return;
            if (!TriggeredRules.Contains(ruleName))
            {
                TriggeredRules.Add(ruleName);
            }
        }
    }
}
