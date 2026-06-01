// Окно редактора графовых правил. Тулбар с метаданными правила (имя, приоритет,
// категория), полотно графа и кнопки сохранения/сборки. Открывается двойным кликом
// по ресурсу графа.
using ExpertSystem.Rules.Graph;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace ExpertSystem.Editor.GraphView
{
    /// <summary>Окно визуального конструктора правил.</summary>
    public class RuleGraphWindow : EditorWindow
    {
        [SerializeField] private RuleGraphAsset _asset;

        private RuleGraphView _graphView;
        private TextField _nameField;
        private IntegerField _priorityField;
        private TextField _categoryField;

        /// <summary>Открывает окно для заданного ресурса графа. Принимает ресурс графа.</summary>
        public static void Open(RuleGraphAsset asset)
        {
            var window = GetWindow<RuleGraphWindow>();
            window.titleContent = new GUIContent("Rule Graph");
            window.LoadAsset(asset);
            window.Show();
        }

        /// <summary>Обработчик двойного клика по ассету. Принимает id и строку; открывает
        /// окно, если ассет — граф правил. Возвращает true, если клик обработан.</summary>
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

        /// <summary>Строит интерфейс окна: тулбар, поля метаданных, полотно графа.</summary>
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

            // Save — сохранить граф в ресурс; Build — сохранить и сгенерировать код.
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

        /// <summary>Запоминает ресурс и, если окно уже построено, отображает его.</summary>
        private void LoadAsset(RuleGraphAsset asset)
        {
            _asset = asset;
            if (_graphView != null) ApplyAsset(asset);
        }

        /// <summary>Заполняет поля метаданных и загружает граф в полотно.</summary>
        private void ApplyAsset(RuleGraphAsset asset)
        {
            _nameField.SetValueWithoutNotify(asset.ruleName);
            _priorityField.SetValueWithoutNotify(asset.priority);
            _categoryField.SetValueWithoutNotify(asset.category);
            _graphView.Load(asset);
        }
    }
}
