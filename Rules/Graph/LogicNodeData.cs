using System;
using UnityEngine;

namespace ExpertSystem.Rules.Graph
{
    [Serializable]
    public class LogicNodeData
    {
        public string id;
        public LogicKind kind;
        public string parentId;
        public Vector2 position;
    }
}
