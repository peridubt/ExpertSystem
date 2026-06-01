// Узел действия в графе. Тип действия выбирается из списка, под него динамически
// перестраиваются поля параметров. Имеет входной порт для подключения к корню.
using System;
using ExpertSystem.Rules.Graph;
using ExpertSystem.RuleEngine.Core.Domain;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ExpertSystem.Editor.GraphView.Nodes
{
    /// <summary>Визуальный узел одного действия (блок Then).</summary>
    public class ActionNodeView : Node
    {
        public ActionData Data { get; }
        public Port InputPort { get; }

        private readonly VisualElement _paramsContainer;

        /// <summary>Строит узел из данных действия. Принимает модель ActionData.</summary>
        public ActionNodeView(ActionData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            title = "Action";

            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            InputPort.portName = "Then";
            inputContainer.Add(InputPort);

            var kindField = new EnumField("Kind", data.kind);
            kindField.RegisterValueChangedCallback(evt =>
            {
                Data.kind = (ActionKind)evt.newValue;
                RebuildParams();
            });
            extensionContainer.Add(kindField);

            _paramsContainer = new VisualElement();
            extensionContainer.Add(_paramsContainer);
            RebuildParams();

            SetPosition(new Rect(data.position, new Vector2(240, 0)));
            RefreshExpandedState();
            RefreshPorts();
        }

        /// <summary>Перестраивает поля параметров под выбранный тип действия.</summary>
        private void RebuildParams()
        {
            _paramsContainer.Clear();

            switch (Data.kind)
            {
                case ActionKind.Suggest:
                    // Предложение действия: тактическое действие, полезность, обоснование.
                    var actionEnum = TryParseEnum<TacticalAction>(Data.tacticalAction);
                    Data.tacticalAction = actionEnum.ToString();

                    var actionField = new EnumField("Action", actionEnum);
                    actionField.RegisterValueChangedCallback(e => Data.tacticalAction = e.newValue.ToString());
                    _paramsContainer.Add(actionField);

                    var scoreField = new IntegerField("Utility") { value = Data.utilityScore };
                    scoreField.RegisterValueChangedCallback(e => Data.utilityScore = e.newValue);
                    _paramsContainer.Add(scoreField);

                    var reasonField = new TextField("Reason") { value = Data.reason ?? string.Empty };
                    reasonField.RegisterValueChangedCallback(e => Data.reason = e.newValue);
                    _paramsContainer.Add(reasonField);
                    break;

                case ActionKind.SetStatus:
                    // Установка статуса решения.
                    var statusEnum = TryParseEnum<DecisionStatus>(Data.decisionStatus);
                    Data.decisionStatus = statusEnum.ToString();

                    var statusField = new EnumField("Status", statusEnum);
                    statusField.RegisterValueChangedCallback(e => Data.decisionStatus = e.newValue.ToString());
                    _paramsContainer.Add(statusField);
                    break;

                case ActionKind.Info:
                case ActionKind.Warning:
                case ActionKind.Error:
                    // Оповещение: одно текстовое поле сообщения.
                    var msgField = new TextField("Message") { value = Data.message ?? string.Empty };
                    msgField.RegisterValueChangedCallback(e => Data.message = e.newValue);
                    _paramsContainer.Add(msgField);
                    break;

                case ActionKind.Update:
                    _paramsContainer.Add(new Label("Обновляет факт решения в рабочей памяти."));
                    break;
            }
        }

        /// <summary>Безопасный разбор строки в значение перечисления. Возвращает значение или default.</summary>
        private static T TryParseEnum<T>(string s) where T : struct, Enum
        {
            return Enum.TryParse<T>(s, out var v) ? v : default;
        }

        /// <summary>Запоминает позицию узла в данных при перемещении.</summary>
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Data.position = newPos.position;
        }
    }
}
