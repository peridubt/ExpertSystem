// Плоский выходной DTO: результат вывода для внешнего потребителя.
using System.Collections.Generic;
using ExpertSystem.RuleEngine.Core.Domain;

namespace ExpertSystem.RuleEngine.Core.Application
{
    /// <summary>Итог работы движка: рекомендованное действие, статус и оповещения.</summary>
    public class DecisionResult
    {
        public string DecisionId { get; set; } = string.Empty;
        public string ActorId { get; set; } = string.Empty;
        public string TargetId { get; set; } = string.Empty;
        public DecisionStatus Status { get; set; }
        public TacticalAction SuggestedAction { get; set; }
        public int UtilityScore { get; set; }
        public string TacticalReason { get; set; } = string.Empty;

        // Оповещения и имена сработавших правил — для трассировки вывода.
        public List<AlertResult> Alerts { get; } = new List<AlertResult>();
        public List<string> TriggeredRules { get; } = new List<string>();
    }

    /// <summary>Оповещение в плоском виде: важность, правило-источник и текст.</summary>
    public class AlertResult
    {
        public int Severity { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
