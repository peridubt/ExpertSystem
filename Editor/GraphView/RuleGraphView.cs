using System;
using System.Collections.Generic;
using System.Linq;
using ExpertSystem.Editor.GraphView.Nodes;
using ExpertSystem.Rules.Graph;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ExpertSystem.Editor.GraphView
{
    public class RuleGraphView : UnityEditor.Experimental.GraphView.GraphView
    {
        private RuleGraphAsset _asset;
        private RuleRootNodeView _root;

        public RuleGraphView()
        {
            style.flexGrow = 1;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(p =>
                p != startPort &&
                p.node != startPort.node &&
                p.direction != startPort.direction &&
                p.portType == startPort.portType
            ).ToList();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var mousePos = contentViewContainer.WorldToLocal(evt.mousePosition);

            evt.menu.AppendAction("Add Condition", _ => AddCondition(mousePos));
            evt.menu.AppendAction("Add Action", _ => AddAction(mousePos));
            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Add Logic/AND", _ => AddLogic(mousePos, LogicKind.And));
            evt.menu.AppendAction("Add Logic/OR", _ => AddLogic(mousePos, LogicKind.Or));
            evt.menu.AppendAction("Add Logic/NOT", _ => AddLogic(mousePos, LogicKind.Not));
            evt.menu.AppendSeparator();

            base.BuildContextualMenu(evt);
        }

        public void Load(RuleGraphAsset asset)
        {
            _asset = asset;

            DeleteElements(graphElements.ToList());
            _root = new RuleRootNodeView();
            AddElement(_root);

            if (asset == null) return;

            var conditionViews = new Dictionary<string, ConditionNodeView>();
            var logicViews = new Dictionary<string, LogicNodeView>();

            foreach (var c in asset.conditions)
            {
                var v = AddConditionView(c, connect: false);
                if (!string.IsNullOrEmpty(c.id)) conditionViews[c.id] = v;
            }
            if (asset.logicNodes != null)
            {
                foreach (var l in asset.logicNodes)
                {
                    var v = AddLogicView(l);
                    if (!string.IsNullOrEmpty(l.id)) logicViews[l.id] = v;
                }
            }
            foreach (var a in asset.actions)
            {
                AddActionView(a, connect: true);
            }

            if (asset.logicNodes != null)
            {
                foreach (var l in asset.logicNodes)
                {
                    var view = logicViews[l.id];
                    var target = ResolveInputPort(l.parentId, logicViews);
                    if (target != null) AddElement(view.OutputPort.ConnectTo(target));
                }
            }

            foreach (var c in asset.conditions)
            {
                var view = conditionViews[c.id];
                var target = ResolveInputPort(c.parentId, logicViews);
                if (target != null) AddElement(view.OutputPort.ConnectTo(target));
            }
        }

        public void Save()
        {
            if (_asset == null) return;

            _asset.conditions.Clear();
            _asset.logicNodes.Clear();
            _asset.actions.Clear();

            var parentMap = new Dictionary<string, string>();
            foreach (var edge in edges.ToList())
            {
                var source = edge.output?.node;
                var target = edge.input?.node;
                if (source == null || target == null) continue;

                var sourceId = GetNodeId(source);
                if (string.IsNullOrEmpty(sourceId)) continue;

                var parentId = target is RuleRootNodeView ? string.Empty : GetNodeId(target) ?? string.Empty;
                parentMap[sourceId] = parentId;
            }

            foreach (var node in nodes.ToList())
            {
                switch (node)
                {
                    case ConditionNodeView c:
                        c.Data.parentId = parentMap.TryGetValue(c.Data.id, out var cp) ? cp : string.Empty;
                        _asset.conditions.Add(c.Data);
                        break;
                    case LogicNodeView l:
                        l.Data.parentId = parentMap.TryGetValue(l.Data.id, out var lp) ? lp : string.Empty;
                        _asset.logicNodes.Add(l.Data);
                        break;
                    case ActionNodeView a:
                        _asset.actions.Add(a.Data);
                        break;
                }
            }

            EditorUtility.SetDirty(_asset);
            AssetDatabase.SaveAssets();
        }

        private void AddCondition(Vector2 pos)
        {
            var data = new ConditionData
            {
                id = Guid.NewGuid().ToString(),
                position = pos,
                op = ComparisonOperator.LessOrEqual,
            };
            AddConditionView(data, connect: true);
        }

        private void AddAction(Vector2 pos)
        {
            var data = new ActionData
            {
                id = Guid.NewGuid().ToString(),
                position = pos,
                kind = ActionKind.Suggest,
            };
            AddActionView(data, connect: true);
        }

        private void AddLogic(Vector2 pos, LogicKind kind)
        {
            var data = new LogicNodeData
            {
                id = Guid.NewGuid().ToString(),
                position = pos,
                kind = kind,
            };
            var view = AddLogicView(data);
            if (_root != null)
            {
                var edge = view.OutputPort.ConnectTo(_root.ConditionsInput);
                AddElement(edge);
            }
        }

        private ConditionNodeView AddConditionView(ConditionData data, bool connect)
        {
            var view = new ConditionNodeView(data);
            AddElement(view);

            if (connect && _root != null)
            {
                var edge = view.OutputPort.ConnectTo(_root.ConditionsInput);
                AddElement(edge);
            }
            return view;
        }

        private LogicNodeView AddLogicView(LogicNodeData data)
        {
            var view = new LogicNodeView(data);
            AddElement(view);
            return view;
        }

        private ActionNodeView AddActionView(ActionData data, bool connect)
        {
            var view = new ActionNodeView(data);
            AddElement(view);

            if (connect && _root != null)
            {
                var edge = _root.ActionsOutput.ConnectTo(view.InputPort);
                AddElement(edge);
            }
            return view;
        }

        private Port ResolveInputPort(string parentId, Dictionary<string, LogicNodeView> logicViews)
        {
            if (string.IsNullOrEmpty(parentId)) return _root?.ConditionsInput;
            if (logicViews.TryGetValue(parentId, out var v)) return v.InputPort;
            return _root?.ConditionsInput;
        }

        private static string GetNodeId(Node n)
        {
            switch (n)
            {
                case ConditionNodeView c: return c.Data.id;
                case LogicNodeView l: return l.Data.id;
                case ActionNodeView a: return a.Data.id;
                default: return null;
            }
        }
    }
}
