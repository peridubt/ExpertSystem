using System;
using UnityEngine;

namespace ExpertSystem.Rules.Graph
{
    [Serializable]
    public class ActionData
    {
        public string id;
        public ActionKind kind;

        public string tacticalAction;
        public int utilityScore;
        public string reason;

        public string decisionStatus;

        public string message;

        public Vector2 position;
    }
}
