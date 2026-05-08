using System;
using ExpertSystem.Rules.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ExpertSystem.Editor.GraphView.Nodes
{
    public class LogicNodeView : Node
    {
        public LogicNodeData Data { get; }
        public Port InputPort { get; }
        public Port OutputPort { get; }

        public LogicNodeView(LogicNodeData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            UpdateTitle();

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

        private void UpdateTitle()
        {
            title = Data.kind.ToString().ToUpperInvariant();
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Data.position = newPos.position;
        }
    }
}
