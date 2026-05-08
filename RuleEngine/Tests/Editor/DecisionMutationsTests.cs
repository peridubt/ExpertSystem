using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using NUnit.Framework;

namespace ExpertSystem.RuleEngine.Tests.Editor
{
    public class DecisionMutationsTests
    {
        [Test]
        public void Suggest_HigherScore_UpdatesDecisionFields()
        {
            var decision = new GameDecision
            {
                UtilityScore = 10,
                SuggestedAction = TacticalAction.Hold,
                TacticalReason = "Initial",
            };

            DecisionMutations.Suggest(
                decision,
                TacticalAction.Burst,
                70,
                "High value burst window",
                "Finisher burst");

            Assert.That(decision.UtilityScore, Is.EqualTo(70));
            Assert.That(decision.SuggestedAction, Is.EqualTo(TacticalAction.Burst));
            Assert.That(decision.TacticalReason, Is.EqualTo("High value burst window"));
            Assert.That(decision.TriggeredRules, Does.Contain("Finisher burst"));
        }

        [Test]
        public void Suggest_LowerScore_KeepsCurrentBestAction()
        {
            var decision = new GameDecision
            {
                UtilityScore = 80,
                SuggestedAction = TacticalAction.HealSelf,
                TacticalReason = "Stay alive",
            };

            DecisionMutations.Suggest(
                decision,
                TacticalAction.Attack,
                50,
                "Chip damage",
                "Focus high-threat target");

            Assert.That(decision.UtilityScore, Is.EqualTo(80));
            Assert.That(decision.SuggestedAction, Is.EqualTo(TacticalAction.HealSelf));
            Assert.That(decision.TacticalReason, Is.EqualTo("Stay alive"));
            Assert.That(decision.TriggeredRules, Does.Contain("Focus high-threat target"));
        }

        [Test]
        public void SetStatus_UpdatesStatus_AndTracksRule()
        {
            var decision = new GameDecision
            {
                Status = DecisionStatus.Open,
            };

            var updated = DecisionMutations.SetStatus(decision, DecisionStatus.Review, "Review decision");

            Assert.That(updated, Is.EqualTo(DecisionStatus.Review));
            Assert.That(decision.Status, Is.EqualTo(DecisionStatus.Review));
            Assert.That(decision.TriggeredRules, Does.Contain("Review decision"));
        }
    }
}