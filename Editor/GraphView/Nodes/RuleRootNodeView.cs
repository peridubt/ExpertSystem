using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ExpertSystem.Editor.GraphView.Nodes
{
    public class RuleRootNodeView : Node
    {
        public Port ConditionsInput { get; }
        public Port ActionsOutput { get; }

        public RuleRootNodeView()
        {
            title = "Rule";
            capabilities &= ~Capabilities.Deletable;

            ConditionsInput = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            ConditionsInput.portName = "When";
            inputContainer.Add(ConditionsInput);

            ActionsOutput = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            ActionsOutput.portName = "Then";
            outputContainer.Add(ActionsOutput);

            RefreshExpandedState();
            RefreshPorts();

            SetPosition(new Rect(400, 200, 200, 100));
        }
    }
}
