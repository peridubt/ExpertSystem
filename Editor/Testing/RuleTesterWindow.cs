// Окно ручной отладки правил. Позволяет задать значения фактов вручную, прогнать
// движок и увидеть, какие правила сработали и каков результат решения.
using System;
using System.Text;
using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Runtime;
using ExpertSystem.Rules.CombatRules;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ExpertSystem.Editor.Testing
{
    /// <summary>Окно интерактивной проверки правил на произвольном состоянии.</summary>
    public class RuleTesterWindow : EditorWindow
    {
        /// <summary>Пункт меню: открыть окно тестера.</summary>
        [MenuItem("Tools/Expert System/Rule Tester")]
        public static void ShowWindow()
        {
            var window = GetWindow<RuleTesterWindow>();
            window.titleContent = new GUIContent("Rule Tester");
            window.minSize = new Vector2(420, 600);
            window.Show();
        }

        private CombatState _state = NewDefaultState();
        private DecisionStatus _initialStatus = DecisionStatus.Open;
        private Label _outputLabel;

        /// <summary>Создаёт состояние боя по умолчанию (полное здоровье, цель цела).</summary>
        private static CombatState NewDefaultState()
        {
            return new CombatState
            {
                PlayerHealthPercent = 100,
                PlayerManaPercent = 100,
                TargetHealthPercent = 100,
                HasLineOfSight = true,
            };
        }

        /// <summary>Строит интерфейс: поля ввода фактов, кнопки и панель вывода.</summary>
        private void CreateGUI()
        {
            var root = rootVisualElement;

            var header = new Label("Rule Tester")
            {
                style =
                {
                    fontSize = 16,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginTop = 8, marginLeft = 8, marginBottom = 4,
                },
            };
            root.Add(header);

            var hint = new Label("Задайте значения фактов, нажмите Evaluate, посмотрите сработавшие правила.")
            {
                style = { marginLeft = 8, marginBottom = 6, unityFontStyleAndWeight = FontStyle.Italic },
            };
            root.Add(hint);

            var scroll = new ScrollView { style = { flexGrow = 1 } };
            root.Add(scroll);

            scroll.Add(SectionHeader("Combat State (числовые)"));
            scroll.Add(IntField("Player HP %", _state.PlayerHealthPercent, v => _state.PlayerHealthPercent = v));
            scroll.Add(IntField("Player Mana %", _state.PlayerManaPercent, v => _state.PlayerManaPercent = v));
            scroll.Add(IntField("Target HP %", _state.TargetHealthPercent, v => _state.TargetHealthPercent = v));
            scroll.Add(IntField("Target Threat %", _state.TargetThreatPercent, v => _state.TargetThreatPercent = v));
            scroll.Add(IntField("Lowest Ally HP %", _state.LowestAllyHealthPercent, v => _state.LowestAllyHealthPercent = v));
            scroll.Add(IntField("Nearby Allies", _state.NearbyAllyCount, v => _state.NearbyAllyCount = v));
            scroll.Add(IntField("Nearby Enemies", _state.NearbyEnemyCount, v => _state.NearbyEnemyCount = v));
            scroll.Add(IntField("Strong Enemies", _state.StrongEnemyCount, v => _state.StrongEnemyCount = v));

            scroll.Add(SectionHeader("Combat State (флаги)"));
            scroll.Add(BoolField("Has Healing Potion", _state.HasHealingPotion, v => _state.HasHealingPotion = v));
            scroll.Add(BoolField("Interrupt Skill Ready", _state.InterruptSkillReady, v => _state.InterruptSkillReady = v));
            scroll.Add(BoolField("Escape Skill Ready", _state.EscapeSkillReady, v => _state.EscapeSkillReady = v));
            scroll.Add(BoolField("Target Is Boss", _state.TargetIsBoss, v => _state.TargetIsBoss = v));
            scroll.Add(BoolField("Target Casting High Impact", _state.TargetIsCastingHighImpactSkill, v => _state.TargetIsCastingHighImpactSkill = v));
            scroll.Add(BoolField("Is Stunned", _state.IsStunned, v => _state.IsStunned = v));
            scroll.Add(BoolField("Is Silenced", _state.IsSilenced, v => _state.IsSilenced = v));
            scroll.Add(BoolField("Is Rooted", _state.IsRooted, v => _state.IsRooted = v));
            scroll.Add(BoolField("Has Line Of Sight", _state.HasLineOfSight, v => _state.HasLineOfSight = v));

            scroll.Add(SectionHeader("Decision"));
            var statusEnum = new EnumField("Initial Status", _initialStatus);
            statusEnum.RegisterValueChangedCallback(e => _initialStatus = (DecisionStatus)e.newValue);
            scroll.Add(statusEnum);

            var btnRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 8, marginLeft = 8, marginRight = 8 } };
            var btn = new Button(Evaluate) { text = "Evaluate", style = { flexGrow = 1, height = 32 } };
            var resetBtn = new Button(Reset) { text = "Reset", style = { width = 80, height = 32, marginLeft = 4 } };
            btnRow.Add(btn);
            btnRow.Add(resetBtn);
            root.Add(btnRow);

            _outputLabel = new Label
            {
                style =
                {
                    whiteSpace = WhiteSpace.Normal,
                    paddingLeft = 8, paddingRight = 8, paddingTop = 8, paddingBottom = 8,
                    marginTop = 8, marginLeft = 8, marginRight = 8, marginBottom = 8,
                    minHeight = 140,
                    backgroundColor = new Color(0f, 0f, 0f, 0.25f),
                },
            };
            _outputLabel.text = "(вывода ещё не было)";
            root.Add(_outputLabel);
        }

        /// <summary>Создаёт жирный заголовок секции. Принимает текст, возвращает Label.</summary>
        private static Label SectionHeader(string text)
        {
            return new Label(text)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginTop = 8, marginLeft = 8, marginBottom = 2,
                },
            };
        }

        /// <summary>Создаёт целочисленное поле. Принимает подпись, начальное значение и сеттер.</summary>
        private static IntegerField IntField(string label, int initial, Action<int> setter)
        {
            var f = new IntegerField(label) { value = initial };
            f.style.marginLeft = 8;
            f.style.marginRight = 8;
            f.RegisterValueChangedCallback(e => setter(e.newValue));
            return f;
        }

        /// <summary>Создаёт переключатель. Принимает подпись, начальное значение и сеттер.</summary>
        private static Toggle BoolField(string label, bool initial, Action<bool> setter)
        {
            var f = new Toggle(label) { value = initial };
            f.style.marginLeft = 8;
            f.style.marginRight = 8;
            f.RegisterValueChangedCallback(e => setter(e.newValue));
            return f;
        }

        /// <summary>Сбрасывает состояние к значениям по умолчанию.</summary>
        private void Reset()
        {
            _state = NewDefaultState();
            _initialStatus = DecisionStatus.Open;
            _outputLabel.text = "(состояние сброшено — переоткройте окно для обновления полей)";
        }

        /// <summary>Прогоняет движок на текущем состоянии и выводит результат в панель.</summary>
        private void Evaluate()
        {
            try
            {
                var decision = new GameDecision
                {
                    DecisionId = "TESTER",
                    ActorId = "Player",
                    Status = _initialStatus,
                };

                var engine = new GameRulesEngine(typeof(LowHealthRule).Assembly);
                var result = engine.Evaluate(decision, _state, new NullNotificationService());

                var sb = new StringBuilder();
                sb.AppendLine($"Suggested action : {result.SuggestedAction}");
                sb.AppendLine($"Utility score    : {result.UtilityScore}");
                sb.AppendLine($"Decision status  : {result.Status}");
                sb.AppendLine($"Reason           : {(string.IsNullOrEmpty(result.TacticalReason) ? "—" : result.TacticalReason)}");
                sb.AppendLine();

                sb.AppendLine($"Сработавшие правила ({result.TriggeredRules.Count}):");
                if (result.TriggeredRules.Count == 0)
                {
                    sb.AppendLine("  (нет — ни одно условие не выполнилось)");
                }
                else
                {
                    foreach (var r in result.TriggeredRules) sb.AppendLine("  • " + r);
                }

                if (result.Alerts.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine($"Оповещения ({result.Alerts.Count}):");
                    foreach (var a in result.Alerts)
                    {
                        sb.AppendLine($"  [sev={a.Severity}] {a.RuleName}: {a.Message}");
                    }
                }

                _outputLabel.text = sb.ToString();
            }
            catch (Exception ex)
            {
                _outputLabel.text = "ОШИБКА: " + ex.Message + "\n\nСм. стек в консоли.";
                Debug.LogException(ex);
            }
        }
    }
}
