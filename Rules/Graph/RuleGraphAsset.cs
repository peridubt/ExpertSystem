using System.Collections.Generic;
using UnityEngine;

namespace ExpertSystem.Rules.Graph
{
    [CreateAssetMenu(fileName = "NewRuleGraph", menuName = "Expert System/Rule Graph", order = 0)]
    public class RuleGraphAsset : ScriptableObject
    {
        public string ruleName = "NewRule";
        public string description = string.Empty;
        public int priority = 0;
        public string category = "Custom";
        public List<ConditionData> conditions = new List<ConditionData>();
        public List<LogicNodeData> logicNodes = new List<LogicNodeData>();
        public List<ActionData> actions = new List<ActionData>();
    }
}
