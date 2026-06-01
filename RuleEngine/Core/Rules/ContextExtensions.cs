// Методы-расширения для контекста правила NRules. Через них правила публикуют
// оповещения: создают факт GameAlert в рабочей памяти, на который реагируют мета-правила.
using ExpertSystem.RuleEngine.Core.Domain;
using NRules.RuleModel;

namespace ExpertSystem.RuleEngine.Core.Rules
{
    /// <summary>Помощники для выпуска оповещений из Then-блоков правил.</summary>
    public static class ContextExtensions
    {
        /// <summary>Информационное оповещение (важность 1). Принимает контекст, решение и текст.</summary>
        public static void Info(this IContext context, GameDecision decision, string message)
        {
            InsertAlert(context, 1, decision, message);
        }

        /// <summary>Предупреждение (важность 2). Поднимает статус решения до Review.</summary>
        public static void Warning(this IContext context, GameDecision decision, string message)
        {
            InsertAlert(context, 2, decision, message);
        }

        /// <summary>Ошибка (важность 3). Приводит к прерыванию решения (Abort).</summary>
        public static void Error(this IContext context, GameDecision decision, string message)
        {
            InsertAlert(context, 3, decision, message);
        }

        /// <summary>
        /// Создаёт факт оповещения и вставляет его в рабочую память. Принимает контекст,
        /// уровень важности, решение и текст. Вставка нового факта запускает переоценку
        /// правил — так срабатывают мета-правила, реагирующие на оповещения.
        /// </summary>
        private static void InsertAlert(IContext context, int severity, GameDecision decision, string message)
        {
            decision.AddTriggeredRule(context.Rule.Name);
            var alert = new GameAlert
            {
                Severity = severity,
                Decision = decision,
                RuleName = context.Rule.Name,
                Message = message,
            };
            context.Insert(alert);
        }
    }
}
