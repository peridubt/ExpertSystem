// Факт оповещения. Через него правила косвенно взаимодействуют: одно правило
// вставляет оповещение в рабочую память, мета-правила реагируют на его наличие.
using ExpertSystem.RuleEngine.Core.Rules;

namespace ExpertSystem.RuleEngine.Core.Domain
{
    /// <summary>Оповещение, выпущенное правилом во время вывода.</summary>
    public class GameAlert
    {
        public int Severity { get; set; }                      // важность: 1 — инфо, 2 — предупреждение, 3 — ошибка
        public GameDecision Decision { get; set; } = null!;    // решение, к которому относится оповещение
        public string RuleName { get; set; } = string.Empty;   // имя правила-источника
        public string Message { get; set; } = string.Empty;    // текст оповещения
    }
}
