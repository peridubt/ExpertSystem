// Окно редактора скриптовых правил. Поля метаданных и многострочный редактор тела
// метода Define(); кнопки сохранения и сборки. Открывается двойным кликом по ресурсу.
using ExpertSystem.Rules.Scripted;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace ExpertSystem.Editor.Scripting
{
    /// <summary>Окно текстового конструктора правил.</summary>
    public class ScriptedRuleWindow : EditorWindow
    {
        [SerializeField] private ScriptedRuleAsset _asset;

        private TextField _nameField;
        private IntegerField _priorityField;
        private TextField _categoryField;
        private TextField _bodyField;

        /// <summary>Открывает окно для заданного ресурса. Принимает ресурс скриптового правила.</summary>
        public static void Open(ScriptedRuleAsset asset)
        {
            var window = GetWindow<ScriptedRuleWindow>();
            window.titleContent = new GUIContent("Scripted Rule");
            window.LoadAsset(asset);
            window.Show();
        }

        /// <summary>Обработчик двойного клика по ассету. Открывает окно, если это скриптовое
        /// правило. Возвращает true, если клик обработан.</summary>
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

        /// <summary>Строит интерфейс: тулбар, подсказку и многострочный редактор тела.</summary>
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

            // Save — сохранить ресурс; Build — сохранить и сгенерировать код.
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

            var hint = new Label("Тело Define() — объявите переменные-факты, затем цепочки When()/Then().");
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
            // Моноширинный шрифт для удобства редактирования кода.
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

        /// <summary>Запоминает ресурс и, если окно построено, отображает его.</summary>
        private void LoadAsset(ScriptedRuleAsset asset)
        {
            _asset = asset;
            if (_bodyField != null) ApplyAsset(asset);
        }

        /// <summary>Заполняет поля окна значениями из ресурса.</summary>
        private void ApplyAsset(ScriptedRuleAsset asset)
        {
            _nameField.SetValueWithoutNotify(asset.ruleName);
            _priorityField.SetValueWithoutNotify(asset.priority);
            _categoryField.SetValueWithoutNotify(asset.category);
            _bodyField.SetValueWithoutNotify(asset.defineBody ?? string.Empty);
        }

        /// <summary>Помечает ресурс изменённым и сохраняет ассеты на диск.</summary>
        private void Save()
        {
            if (_asset == null) return;
            EditorUtility.SetDirty(_asset);
            AssetDatabase.SaveAssets();
        }
    }
}
