using System;
using ExpertSystem.Rules.Graph;
using ExpertSystem.RuleEngine.Core.Domain;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ExpertSystem.Editor.GraphView.Nodes
{
    public class ActionNodeView : Node
    {
        public ActionData Data { get; }
        public Port InputPort { get; }

        private readonly VisualElement _paramsContainer;

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

        private void RebuildParams()
        {
            _paramsContainer.Clear();

            switch (Data.kind)
            {
                case ActionKind.Suggest:
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
                    var statusEnum = TryParseEnum<DecisionStatus>(Data.decisionStatus);
                    Data.decisionStatus = statusEnum.ToString();

                    var statusField = new EnumField("Status", statusEnum);
                    statusField.RegisterValueChangedCallback(e => Data.decisionStatus = e.newValue.ToString());
                    _paramsContainer.Add(statusField);
                    break;

                case ActionKind.Info:
                case ActionKind.Warning:
                case ActionKind.Error:
                    var msgField = new TextField("Message") { value = Data.message ?? string.Empty };
                    msgField.RegisterValueChangedCallback(e => Data.message = e.newValue);
                    _paramsContainer.Add(msgField);
                    break;

                case ActionKind.Update:
                    _paramsContainer.Add(new Label("Updates the decision fact in working memory."));
                    break;
            }
        }

        private static T TryParseEnum<T>(string s) where T : struct, Enum
        {
            return Enum.TryParse<T>(s, out var v) ? v : default;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Data.position = newPos.position;
        }
    }
}
