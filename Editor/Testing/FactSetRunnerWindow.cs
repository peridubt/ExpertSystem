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
    public class FactSetRunnerWindow : EditorWindow
    {
        [MenuItem("Tools/Expert System/Fact Set Runner")]
        public static void ShowWindow()
        {
            var window = GetWindow<FactSetRunnerWindow>();
            window.titleContent = new GUIContent("Fact Set Runner");
            window.minSize = new Vector2(520, 600);
            window.Show();
        }

        private ExpertSystemDataService _service;
        private List<string> _factSetIds = new List<string>();
        private string _selectedId;
        private FactSetData _loaded;
        private DecisionResult _lastResult;

        private DropdownField _idDropdown;
        private Label _factsPreview;
        private Label _resultLabel;
        private Button _evaluateButton;
        private Button _saveResultButton;

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

            var hint = new Label("Loads fact JSONs from Assets/ExpertSystem/Data/Facts/, runs the engine, shows the decision.")
            {
                style = { unityFontStyleAndWeight = FontStyle.Italic, marginBottom = 8 },
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
            _factsPreview = new Label("(no fact set loaded)")
            {
                style =
                {
                    whiteSpace = WhiteSpace.Normal,
                    paddingLeft = 8, paddingRight = 8, paddingTop = 6, paddingBottom = 6,
                    backgroundColor = new Color(0f, 0f, 0f, 0.2f),
                    minHeight = 100,
                },
            };
            root.Add(_factsPreview);

            var btnRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 8 } };
            _evaluateButton = new Button(Evaluate)
            {
                text = "Evaluate",
                style = { flexGrow = 1, height = 32 },
            };
            _saveResultButton = new Button(SaveResult)
            {
                text = "Save Result to JSON",
                style = { width = 180, height = 32, marginLeft = 4 },
            };
            _saveResultButton.SetEnabled(false);
            btnRow.Add(_evaluateButton);
            btnRow.Add(_saveResultButton);
            root.Add(btnRow);

            root.Add(SectionHeader("Decision result"));
            _resultLabel = new Label("(no evaluation yet)")
            {
                style =
                {
                    whiteSpace = WhiteSpace.Normal,
                    paddingLeft = 8, paddingRight = 8, paddingTop = 8, paddingBottom = 8,
                    marginTop = 4,
                    backgroundColor = new Color(0f, 0f, 0f, 0.25f),
                    flexGrow = 1,
                    minHeight = 180,
                },
            };
            root.Add(_resultLabel);

            if (!string.IsNullOrEmpty(_idDropdown.value))
            {
                SelectFactSet(_idDropdown.value);
            }
        }

        private static Label SectionHeader(string text)
        {
            return new Label(text)
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 8, marginBottom = 2 },
            };
        }

        private void RefreshFactSetList()
        {
            _factSetIds = new List<string>(_service.ListFactSetIds());
            if (_factSetIds.Count == 0) _factSetIds.Add("(no fact sets found)");
        }

        private int GetInitialIndex()
        {
            return _factSetIds.Count > 0 && !_factSetIds[0].StartsWith("(") ? 0 : -1;
        }

        private void SelectFactSet(string id)
        {
            _selectedId = id;
            if (string.IsNullOrEmpty(id) || id.StartsWith("("))
            {
                _loaded = null;
                _factsPreview.text = "(no fact set selected)";
                return;
            }

            try
            {
                _loaded = _service.LoadFactSet(id);
                if (_loaded == null)
                {
                    _factsPreview.text = "Failed to load.";
                    return;
                }
                _factsPreview.text = FormatFactsPreview(_loaded);
            }
            catch (Exception ex)
            {
                _factsPreview.text = "ERROR: " + ex.Message;
                Debug.LogException(ex);
            }
        }

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

        private void Evaluate()
        {
            if (_loaded == null)
            {
                _resultLabel.text = "Select a fact set first.";
                return;
            }

            try
            {
                var snapshot = FactSetMapper.ToSnapshot(_loaded);
                var engine = new GameRulesEngine(typeof(LowHealthRule).Assembly);
                var service = new RuleEvaluationService(engine, new NullNotificationService());
                _lastResult = service.Evaluate(snapshot);

                _resultLabel.text = FormatResult(_lastResult);
                _saveResultButton.SetEnabled(true);
            }
            catch (Exception ex)
            {
                _resultLabel.text = "ERROR: " + ex.Message;
                _saveResultButton.SetEnabled(false);
                Debug.LogException(ex);
            }
        }

        private static string FormatResult(DecisionResult r)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Suggested action : {r.SuggestedAction}");
            sb.AppendLine($"Utility score    : {r.UtilityScore}");
            sb.AppendLine($"Decision status  : {r.Status}");
            sb.AppendLine($"Reason           : {(string.IsNullOrEmpty(r.TacticalReason) ? "—" : r.TacticalReason)}");
            sb.AppendLine();
            sb.AppendLine($"Triggered rules ({r.TriggeredRules.Count}):");
            if (r.TriggeredRules.Count == 0) sb.AppendLine("  (none)");
            else foreach (var name in r.TriggeredRules) sb.AppendLine("  • " + name);

            if (r.Alerts.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"Alerts ({r.Alerts.Count}):");
                foreach (var a in r.Alerts)
                    sb.AppendLine($"  [sev={a.Severity}] {a.RuleName}: {a.Message}");
            }
            return sb.ToString();
        }

        private void SaveResult()
        {
            if (_lastResult == null || _loaded == null) return;
            try
            {
                var data = FactSetMapper.ToDataResult(_lastResult, _loaded.factSetId);
                var fileName = _loaded.factSetId + "-result";
                _service.SaveDecisionResult(data, fileName);
                AssetDatabase.Refresh();
                Debug.Log($"[ExpertSystem] Saved result to {Path.Combine(_service.ResultsRoot, fileName + ".json")}");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

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
