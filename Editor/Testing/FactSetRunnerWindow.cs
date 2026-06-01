// Окно прогона наборов фактов. Загружает JSON из Data/Facts, прогоняет движок и
// показывает и решение, и изменения состояния от эффект-правил. Кнопка Step повторно
// подаёт изменённое состояние на вход — моделирует эволюцию во времени.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ExpertSystem.RuleEngine.Core.Application;
using ExpertSystem.RuleEngine.Core.Data;
using ExpertSystem.RuleEngine.Core.Runtime;
using ExpertSystem.Rules.CombatRules;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ExpertSystem.Editor.Testing
{
    /// <summary>Окно загрузки наборов фактов и прогона их через движок.</summary>
    public class FactSetRunnerWindow : EditorWindow
    {
        /// <summary>Пункт меню: открыть окно прогона наборов фактов.</summary>
        [MenuItem("Tools/Expert System/Fact Set Runner")]
        public static void ShowWindow()
        {
            var window = GetWindow<FactSetRunnerWindow>();
            window.titleContent = new GUIContent("Fact Set Runner");
            window.minSize = new Vector2(560, 720);
            window.Show();
        }

        private ExpertSystemDataService _service;
        private RuleEvaluationService _ruleService;
        private List<string> _factSetIds = new List<string>();
        private string _selectedId;
        private FactSetData _loaded;

        private EvaluationOutcome _lastOutcome;
        private CombatSnapshot _currentSnapshot; // эволюционирующее состояние, повторно подаётся кнопкой Step

        private DropdownField _idDropdown;
        private Label _factsPreview;
        private Label _stateLabel;
        private Label _resultLabel;
        private Button _evaluateButton;
        private Button _stepButton;
        private Button _saveResultButton;
        private Button _saveEvolvedButton;

        /// <summary>Строит интерфейс окна и загружает список наборов фактов.</summary>
        private void CreateGUI()
        {
            _service = new ExpertSystemDataService();
            RefreshFactSetList();

            var root = rootVisualElement;
            root.style.paddingLeft = 8;
            root.style.paddingRight = 8;
            root.style.paddingTop = 8;
            root.style.paddingBottom = 8;

            var header = new Label("Fact Set Runner")
            {
                style = { fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 4 },
            };
            root.Add(header);

            var hint = new Label(
                "Загружает наборы фактов из Assets/ExpertSystem/Data/Facts/, прогоняет движок и " +
                "показывает решение и изменения состояния от эффект-правил. " +
                "\"Step\" повторно подаёт изменённое состояние — моделирует эволюцию во времени.")
            {
                style = { unityFontStyleAndWeight = FontStyle.Italic, marginBottom = 8, whiteSpace = WhiteSpace.Normal },
            };
            root.Add(hint);

            var topRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 6 } };
            _idDropdown = new DropdownField("Fact set", _factSetIds, GetInitialIndex())
            {
                style = { flexGrow = 1 },
            };
            _idDropdown.RegisterValueChangedCallback(e => SelectFactSet(e.newValue));
            topRow.Add(_idDropdown);

            var refreshBtn = new Button(() => { RefreshFactSetList(); _idDropdown.choices = _factSetIds; })
            {
                text = "Refresh",
                style = { width = 80, marginLeft = 4 },
            };
            topRow.Add(refreshBtn);

            var revealBtn = new Button(RevealFactsFolder)
            {
                text = "Open Folder",
                style = { width = 100, marginLeft = 4 },
            };
            topRow.Add(revealBtn);
            root.Add(topRow);

            root.Add(SectionHeader("Loaded facts"));
            _factsPreview = new Label("(набор фактов не загружен)")
            {
                style =
                {
                    whiteSpace = WhiteSpace.Normal,
                    paddingLeft = 8, paddingRight = 8, paddingTop = 6, paddingBottom = 6,
                    backgroundColor = new Color(0f, 0f, 0f, 0.2f),
                    minHeight = 90,
                },
            };
            root.Add(_factsPreview);

            var runRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 8 } };
            _evaluateButton = new Button(Evaluate)
            {
                text = "Evaluate",
                style = { flexGrow = 1, height = 32 },
            };
            _stepButton = new Button(Step)
            {
                text = "Step ▶ (re-feed)",
                style = { width = 160, height = 32, marginLeft = 4 },
            };
            _stepButton.SetEnabled(false);
            runRow.Add(_evaluateButton);
            runRow.Add(_stepButton);
            root.Add(runRow);

            root.Add(SectionHeader("State changes (effect rules)"));
            _stateLabel = new Label("(вывода ещё не было)")
            {
                style =
                {
                    whiteSpace = WhiteSpace.Normal,
                    paddingLeft = 8, paddingRight = 8, paddingTop = 6, paddingBottom = 6,
                    marginTop = 4,
                    backgroundColor = new Color(0.1f, 0.15f, 0.1f, 0.35f),
                    minHeight = 70,
                },
            };
            root.Add(_stateLabel);

            root.Add(SectionHeader("Decision result"));
            _resultLabel = new Label("(вывода ещё не было)")
            {
                style =
                {
                    whiteSpace = WhiteSpace.Normal,
                    paddingLeft = 8, paddingRight = 8, paddingTop = 8, paddingBottom = 8,
                    marginTop = 4,
                    backgroundColor = new Color(0f, 0f, 0f, 0.25f),
                    flexGrow = 1,
                    minHeight = 150,
                },
            };
            root.Add(_resultLabel);

            var saveRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 8 } };
            _saveResultButton = new Button(SaveResult)
            {
                text = "Save Result to JSON",
                style = { flexGrow = 1, height = 30 },
            };
            _saveResultButton.SetEnabled(false);
            _saveEvolvedButton = new Button(SaveEvolved)
            {
                text = "Save Evolved Facts",
                style = { flexGrow = 1, height = 30, marginLeft = 4 },
            };
            _saveEvolvedButton.SetEnabled(false);
            saveRow.Add(_saveResultButton);
            saveRow.Add(_saveEvolvedButton);
            root.Add(saveRow);

            if (!string.IsNullOrEmpty(_idDropdown.value))
            {
                SelectFactSet(_idDropdown.value);
            }
        }

        /// <summary>Создаёт жирный заголовок секции. Принимает текст, возвращает Label.</summary>
        private static Label SectionHeader(string text)
        {
            return new Label(text)
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 8, marginBottom = 2 },
            };
        }

        /// <summary>Перечитывает список идентификаторов наборов фактов из хранилища.</summary>
        private void RefreshFactSetList()
        {
            _factSetIds = new List<string>(_service.ListFactSetIds());
            if (_factSetIds.Count == 0) _factSetIds.Add("(наборы фактов не найдены)");
        }

        /// <summary>Возвращает начальный индекс выпадающего списка (-1, если списка нет).</summary>
        private int GetInitialIndex()
        {
            return _factSetIds.Count > 0 && !_factSetIds[0].StartsWith("(") ? 0 : -1;
        }

        /// <summary>
        /// Выбирает набор фактов: сбрасывает состояние эволюции и кнопки, загружает и
        /// показывает превью. Принимает идентификатор набора.
        /// </summary>
        private void SelectFactSet(string id)
        {
            _selectedId = id;
            _currentSnapshot = null;
            _lastOutcome = null;
            _stepButton?.SetEnabled(false);
            _saveResultButton?.SetEnabled(false);
            _saveEvolvedButton?.SetEnabled(false);
            if (_stateLabel != null) _stateLabel.text = "(вывода ещё не было)";
            if (_resultLabel != null) _resultLabel.text = "(вывода ещё не было)";

            if (string.IsNullOrEmpty(id) || id.StartsWith("("))
            {
                _loaded = null;
                _factsPreview.text = "(набор фактов не выбран)";
                return;
            }

            try
            {
                _loaded = _service.LoadFactSet(id);
                if (_loaded == null)
                {
                    _factsPreview.text = "Не удалось загрузить.";
                    return;
                }
                _factsPreview.text = FormatFactsPreview(_loaded);
            }
            catch (Exception ex)
            {
                _factsPreview.text = "ОШИБКА: " + ex.Message;
                Debug.LogException(ex);
            }
        }

        /// <summary>Форматирует краткое превью набора фактов. Принимает DTO, возвращает текст.</summary>
        private static string FormatFactsPreview(FactSetData data)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"id              : {data.factSetId}");
            sb.AppendLine($"description     : {data.description}");
            sb.AppendLine($"actor / target  : {data.actorId} → {data.targetId}");
            var cs = data.combatState;
            sb.AppendLine($"player HP / MP  : {cs.playerHealthPercent}% / {cs.playerManaPercent}%");
            sb.AppendLine($"target HP / thr : {cs.targetHealthPercent}% / threat {cs.targetThreatPercent}%");
            sb.AppendLine($"enemies near    : {cs.nearbyEnemyCount} (strong: {cs.strongEnemyCount}, boss: {cs.targetIsBoss})");
            sb.AppendLine($"flags           : potion={cs.hasHealingPotion}, interrupt={cs.interruptSkillReady}, escape={cs.escapeSkillReady}");
            sb.AppendLine($"states          : stun={cs.isStunned}, silence={cs.isSilenced}, root={cs.isRooted}, casting={cs.targetIsCastingHighImpactSkill}");
            sb.AppendLine($"enemies in list : {(data.enemies?.Count ?? 0)}");
            return sb.ToString();
        }

        /// <summary>Лениво создаёт и кэширует сервис вывода на полной сборке правил.</summary>
        private RuleEvaluationService GetService()
        {
            if (_ruleService == null)
            {
                var engine = new GameRulesEngine(typeof(LowHealthRule).Assembly);
                _ruleService = new RuleEvaluationService(engine, new NullNotificationService());
            }
            return _ruleService;
        }

        /// <summary>Прогоняет выбранный набор фактов с нуля (из загруженного ресурса).</summary>
        private void Evaluate()
        {
            if (_loaded == null)
            {
                _resultLabel.text = "Сначала выберите набор фактов.";
                return;
            }

            try
            {
                var input = FactSetMapper.ToSnapshot(_loaded);
                RunCycle(input, "Evaluate — из загруженного набора фактов");
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        /// <summary>Повторно прогоняет движок по уже изменённому состоянию (следующий тик).</summary>
        private void Step()
        {
            if (_currentSnapshot == null) return;
            try
            {
                RunCycle(_currentSnapshot, "Step — повторная подача изменённого состояния");
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        /// <summary>
        /// Один цикл вывода. Принимает входной снимок и подпись режима. Выводит решение и
        /// дифф состояния, запоминает изменённый снимок для следующего шага.
        /// </summary>
        private void RunCycle(CombatSnapshot input, string mode)
        {
            var outcome = GetService().EvaluateDetailed(input);

            _lastOutcome = outcome;
            _resultLabel.text = FormatResult(outcome.Decision);
            _stateLabel.text = $"[{mode}]\n" + FormatStateDiff(input, outcome.UpdatedSnapshot);
            _currentSnapshot = outcome.UpdatedSnapshot;

            _stepButton.SetEnabled(true);
            _saveResultButton.SetEnabled(true);
            _saveEvolvedButton.SetEnabled(true);
        }

        /// <summary>
        /// Сравнивает поля снимков до и после вывода. Принимает два снимка, возвращает
        /// текст с перечнем изменившихся полей (или сообщение об отсутствии изменений).
        /// </summary>
        private static string FormatStateDiff(CombatSnapshot before, CombatSnapshot after)
        {
            var sb = new StringBuilder();

            // Добавляет строку, только если значение изменилось.
            void Cmp(string label, object a, object b)
            {
                if (!Equals(a, b)) sb.AppendLine($"  {label}: {a} → {b}");
            }

            Cmp("PlayerHealthPercent", before.PlayerHealthPercent, after.PlayerHealthPercent);
            Cmp("PlayerManaPercent", before.PlayerManaPercent, after.PlayerManaPercent);
            Cmp("TargetHealthPercent", before.TargetHealthPercent, after.TargetHealthPercent);
            Cmp("TargetThreatPercent", before.TargetThreatPercent, after.TargetThreatPercent);
            Cmp("NearbyAllyCount", before.NearbyAllyCount, after.NearbyAllyCount);
            Cmp("NearbyEnemyCount", before.NearbyEnemyCount, after.NearbyEnemyCount);
            Cmp("StrongEnemyCount", before.StrongEnemyCount, after.StrongEnemyCount);
            Cmp("LowestAllyHealthPercent", before.LowestAllyHealthPercent, after.LowestAllyHealthPercent);
            Cmp("HasHealingPotion", before.HasHealingPotion, after.HasHealingPotion);
            Cmp("InterruptSkillReady", before.InterruptSkillReady, after.InterruptSkillReady);
            Cmp("EscapeSkillReady", before.EscapeSkillReady, after.EscapeSkillReady);
            Cmp("IsStunned", before.IsStunned, after.IsStunned);
            Cmp("IsSilenced", before.IsSilenced, after.IsSilenced);
            Cmp("IsRooted", before.IsRooted, after.IsRooted);
            Cmp("BurstCooldownSeconds", before.BurstCooldownSeconds, after.BurstCooldownSeconds);
            Cmp("InterruptCooldownSeconds", before.InterruptCooldownSeconds, after.InterruptCooldownSeconds);
            Cmp("EscapeCooldownSeconds", before.EscapeCooldownSeconds, after.EscapeCooldownSeconds);

            if (sb.Length == 0)
            {
                return "(факты не изменились — правила только дали рекомендацию)";
            }
            return sb.ToString();
        }

        /// <summary>Форматирует результат решения для панели вывода. Принимает результат, возвращает текст.</summary>
        private static string FormatResult(DecisionResult r)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Suggested action : {r.SuggestedAction}");
            sb.AppendLine($"Utility score    : {r.UtilityScore}");
            sb.AppendLine($"Decision status  : {r.Status}");
            sb.AppendLine($"Reason           : {(string.IsNullOrEmpty(r.TacticalReason) ? "—" : r.TacticalReason)}");
            sb.AppendLine();
            sb.AppendLine($"Сработавшие правила ({r.TriggeredRules.Count}):");
            if (r.TriggeredRules.Count == 0) sb.AppendLine("  (нет)");
            else foreach (var name in r.TriggeredRules) sb.AppendLine("  • " + name);

            if (r.Alerts.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"Оповещения ({r.Alerts.Count}):");
                foreach (var a in r.Alerts)
                    sb.AppendLine($"  [sev={a.Severity}] {a.RuleName}: {a.Message}");
            }
            return sb.ToString();
        }

        /// <summary>Сохраняет последний результат вывода в JSON (папка Results).</summary>
        private void SaveResult()
        {
            if (_lastOutcome == null || _loaded == null) return;
            try
            {
                var data = FactSetMapper.ToDataResult(_lastOutcome.Decision, _loaded.factSetId);
                var fileName = _loaded.factSetId + "-result";
                _service.SaveDecisionResult(data, fileName);
                AssetDatabase.Refresh();
                Debug.Log($"[ExpertSystem] Результат сохранён в {Path.Combine(_service.ResultsRoot, fileName + ".json")}");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>Сохраняет изменённое состояние как новый набор фактов "&lt;id&gt;-evolved".</summary>
        private void SaveEvolved()
        {
            if (_currentSnapshot == null || _loaded == null) return;
            try
            {
                var id = _loaded.factSetId + "-evolved";
                var data = FactSetMapper.ToData(_currentSnapshot, id);
                data.description = $"Изменённое движком состояние из '{_loaded.factSetId}'.";
                _service.SaveFactSet(data);
                AssetDatabase.Refresh();
                RefreshFactSetList();
                _idDropdown.choices = _factSetIds;
                Debug.Log($"[ExpertSystem] Изменённые факты сохранены в {Path.Combine(_service.FactsRoot, id + ".json")}");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>Выводит сообщение об ошибке в панель и в консоль. Принимает исключение.</summary>
        private void ShowError(Exception ex)
        {
            _resultLabel.text = "ОШИБКА: " + ex.Message + "\n\nСм. стек в консоли.";
            Debug.LogException(ex);
        }

        /// <summary>Открывает папку с наборами фактов в проводнике.</summary>
        private void RevealFactsFolder()
        {
            var path = _service.FactsRoot;
            if (Directory.Exists(path))
            {
                EditorUtility.RevealInFinder(path);
            }
        }
    }
}
