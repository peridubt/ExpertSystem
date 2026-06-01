// Узел логического оператора (AND/OR/NOT) в графе. Имеет вход на несколько связей
// (условия и вложенные группы) и один выход к родителю. Вид оператора выбирается из списка.
using System;
using ExpertSystem.Rules.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ExpertSystem.Editor.GraphView.Nodes
{
    /// <summary>Визуальный узел логической группы.</summary>
    public class LogicNodeView : Node
    {
        public LogicNodeData Data { get; }
        public Port InputPort { get; }
        public Port OutputPort { get; }

        /// <summary>Строит узел из данных логической группы. Принимает модель LogicNodeData.</summary>
        public LogicNodeView(LogicNodeData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            UpdateTitle();

            // Вход — Multi: к группе можно подключить несколько условий/подгрупп.
            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            InputPort.portName = "in";
            inputContainer.Add(InputPort);

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            OutputPort.portName = "out";
            outputContainer.Add(OutputPort);

            var kindField = new EnumField("Kind", data.kind);
            kindField.RegisterValueChangedCallback(evt =>
            {
                Data.kind = (LogicKind)evt.newValue;
                UpdateTitle();
            });
            extensionContainer.Add(kindField);

            SetPosition(new Rect(data.position, new Vector2(160, 0)));
            RefreshExpandedState();
            RefreshPorts();
        }

        /// <summary>Обновляет заголовок узла под выбранный оператор (AND/OR/NOT).</summary>
        private void UpdateTitle()
        {
            title = Data.kind.ToString().ToUpperInvariant();
        }

        /// <summary>Запоминает позицию узла в данных при перемещении.</summary>
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Data.position = newPos.position;
        }
    }
}
