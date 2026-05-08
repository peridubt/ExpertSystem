using ExpertSystem.Rules.Scripted;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace ExpertSystem.Editor.Scripting
{
    public class ScriptedRuleWindow : EditorWindow
    {
        [SerializeField] private ScriptedRuleAsset _asset;

        private TextField _nameField;
        private IntegerField _priorityField;
        private TextField _categoryField;
        private TextField _bodyField;

        public static void Open(ScriptedRuleAsset asset)
        {
            var window = GetWindow<ScriptedRuleWindow>();
            window.titleContent = new GUIContent("Scripted Rule");
            window.LoadAsset(asset);
            window.Show();
        }

        [OnOpenAsset]
        public static bool OnOpen(int instanceID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is ScriptedRuleAsset asset)
            {
                Open(asset);
                return true;
            }
            return false;
        }

        private void CreateGUI()
        {
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.paddingLeft = 6;
            toolbar.style.paddingRight = 6;
            toolbar.style.paddingTop = 4;
            toolbar.style.paddingBottom = 4;

            _nameField = new TextField("Rule Name");
            _nameField.style.flexGrow = 1;
            _nameField.RegisterValueChangedCallback(e =>
            {
                if (_asset == null) return;
                _asset.ruleName = e.newValue;
                EditorUtility.SetDirty(_asset);
            });

            _priorityField = new IntegerField("Priority");
            _priorityField.style.width = 120;
            _priorityField.RegisterValueChangedCallback(e =>
            {
                if (_asset == null) return;
                _asset.priority = e.newValue;
                EditorUtility.SetDirty(_asset);
            });

            _categoryField = new TextField("Category");
            _categoryField.style.width = 160;
            _categoryField.RegisterValueChangedCallback(e =>
            {
                if (_asset == null) return;
                _asset.category = e.newValue;
                EditorUtility.SetDirty(_asset);
            });

            var saveButton = new Button(Save) { text = "Save" };
            var buildButton = new Button(() =>
            {
                Save();
                if (_asset != null) ScriptedRuleCodeGenerator.Build(_asset);
            }) { text = "Build" };

            toolbar.Add(_nameField);
            toolbar.Add(_priorityField);
            toolbar.Add(_categoryField);
            toolbar.Add(saveButton);
            toolbar.Add(buildButton);
            rootVisualElement.Add(toolbar);

            var hint = new Label("Body of Define() — declare fact vars, then When()/Then() chains.");
            hint.style.paddingLeft = 6;
            hint.style.paddingRight = 6;
            hint.style.unityFontStyleAndWeight = FontStyle.Italic;
            rootVisualElement.Add(hint);

            _bodyField = new TextField { multiline = true };
            _bodyField.style.flexGrow = 1;
            _bodyField.style.whiteSpace = WhiteSpace.Normal;
            _bodyField.style.marginLeft = 6;
            _bodyField.style.marginRight = 6;
            _bodyField.style.marginBottom = 6;
            var inner = _bodyField.Q(className: "unity-text-element");
            if (inner != null)
            {
                inner.style.unityFont = (Font)EditorGUIUtility.Load("Fonts/RobotoMono/RobotoMono-Regular.ttf");
                inner.style.whiteSpace = WhiteSpace.Normal;
            }
            _bodyField.RegisterValueChangedCallback(e =>
            {
                if (_asset == null) return;
                _asset.defineBody = e.newValue;
                EditorUtility.SetDirty(_asset);
            });
            rootVisualElement.Add(_bodyField);

            if (_asset != null) ApplyAsset(_asset);
        }

        private void LoadAsset(ScriptedRuleAsset asset)
        {
            _asset = asset;
            if (_bodyField != null) ApplyAsset(asset);
        }

        private void ApplyAsset(ScriptedRuleAsset asset)
        {
            _nameField.SetValueWithoutNotify(asset.ruleName);
            _priorityField.SetValueWithoutNotify(asset.priority);
            _categoryField.SetValueWithoutNotify(asset.category);
            _bodyField.SetValueWithoutNotify(asset.defineBody ?? string.Empty);
        }

        private void Save()
        {
            if (_asset == null) return;
            EditorUtility.SetDirty(_asset);
            AssetDatabase.SaveAssets();
        }
    }
}
