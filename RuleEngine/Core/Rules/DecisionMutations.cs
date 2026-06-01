// Набор операций, которыми Then-блоки правил изменяют решение. Здесь же —
// логика разрешения конфликтов по полезности (выигрывает предложение с большим score).
using ExpertSystem.RuleEngine.Core.Domain;

namespace ExpertSystem.RuleEngine.Core.Rules
{
    /// <summary>Статические мутации факта решения, вызываемые из действий правил.</summary>
    public static class DecisionMutations
    {
        /// <summary>
        /// Предлагает действие. Принимает решение, действие, его полезность, обоснование
        /// и имя правила. Возвращает итоговый score решения. Новое предложение принимается,
        /// только если его score строго больше текущего (правило "побеждает сильнейший").
        /// </summary>
        public static int Suggest(GameDecision decision, TacticalAction action, int score, string reason, string ruleName)
        {
            decision.AddTriggeredRule(ruleName);
            if (score <= decision.UtilityScore) return decision.UtilityScore;

            decision.UtilityScore = score;
            decision.SuggestedAction = action;
            decision.TacticalReason = reason;
            return decision.UtilityScore;
        }

        /// <summary>
        /// Устанавливает статус решения. Принимает решение, новый статус и имя правила.
        /// Возвращает установленный статус.
        /// </summary>
        public static DecisionStatus SetStatus(GameDecision decision, DecisionStatus status, string ruleName)
        {
            decision.AddTriggeredRule(ruleName);
            return decision.Status = status;
        }
    }
}
