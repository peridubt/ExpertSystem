using ExpertSystem.Rules.Graph;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace ExpertSystem.Editor.GraphView
{
    public class RuleGraphWindow : EditorWindow
    {
        [SerializeField] private RuleGraphAsset _asset;

        private RuleGraphView _graphView;
        private TextField _nameField;
        private IntegerField _priorityField;
        private TextField _categoryField;

        public static void Open(RuleGraphAsset asset)
        {
            var window = GetWindow<RuleGraphWindow>();
            window.titleContent = new GUIContent("Rule Graph");
            window.LoadAsset(asset);
            window.Show();
        }

        [OnOpenAsset]
        public static bool OnOpen(int instanceID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is RuleGraphAsset asset)
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

            var saveButton = new Button(() => _graphView.Save()) { text = "Save" };

            var buildButton = new Button(() =>
            {
                _graphView.Save();
                if (_asset != null) RuleCodeGenerator.Build(_asset);
            }) { text = "Build" };

            toolbar.Add(_nameField);
            toolbar.Add(_priorityField);
            toolbar.Add(_categoryField);
            toolbar.Add(saveButton);
            toolbar.Add(buildButton);
            rootVisualElement.Add(toolbar);

            _graphView = new RuleGraphView();
            _graphView.style.flexGrow = 1;
            rootVisualElement.Add(_graphView);

            if (_asset != null) ApplyAsset(_asset);
        }

        private void LoadAsset(RuleGraphAsset asset)
        {
            _asset = asset;
            if (_graphView != null) ApplyAsset(asset);
        }

        private void ApplyAsset(RuleGraphAsset asset)
        {
            _nameField.SetValueWithoutNotify(asset.ruleName);
            _priorityField.SetValueWithoutNotify(asset.priority);
            _categoryField.SetValueWithoutNotify(asset.category);
            _graphView.Load(asset);
        }
    }
}
