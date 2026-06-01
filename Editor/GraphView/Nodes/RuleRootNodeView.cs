// Корневой узел правила. Левый вход собирает условия (блок When), правый выход —
// действия (блок Then). Удалить нельзя: это якорь графа.
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ExpertSystem.Editor.GraphView.Nodes
{
    /// <summary>Центральный узел правила, к которому подключаются условия и действия.</summary>
    public class RuleRootNodeView : Node
    {
        public Port ConditionsInput { get; }
        public Port ActionsOutput { get; }

        /// <summary>Создаёт корневой узел с входом условий и выходом действий.</summary>
        public RuleRootNodeView()
        {
            title = "Rule";
            capabilities &= ~Capabilities.Deletable; // запрет удаления

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
