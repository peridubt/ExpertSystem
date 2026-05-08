using System;
using UnityEngine;

namespace ExpertSystem.Rules.Graph
{
    [Serializable]
    public class ConditionData
    {
        public string id;
        public string factTypeName;
        public string fieldName;
        public ComparisonOperator op;
        public string constantValue;
        public string parentId;
        public Vector2 position;
    }
}
