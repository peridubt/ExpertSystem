using UnityEngine;

namespace ExpertSystem.Rules.Scripted
{
    [CreateAssetMenu(fileName = "NewScriptedRule", menuName = "Expert System/Scripted Rule", order = 1)]
    public class ScriptedRuleAsset : ScriptableObject
    {
        public string ruleName = "NewRule";
        public string description = string.Empty;
        public int priority = 0;
        public string category = "Custom";

        [TextArea(20, 60)]
        public string defineBody =
@"GameDecision decision = null!;

When()
    .Match(() => decision);

Then()
    .Do(ctx => ctx.Update(decision));";
    }
}
