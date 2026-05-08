using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.Rules.DecisionRules;
using NRules.Testing;
using NUnit.Framework;

namespace ExpertSystem.RuleEngine.Tests.Editor
{
    [TestFixture]
    public class ExecuteDecisionRuleTests : IsolatedRulesTestFixture
    {
        public ExecuteDecisionRuleTests()
        {
            Setup.Rule<ExecuteDecisionRule>();
        }

        [Test]
        public void Fires_When_NoWarningOrErrorAlerts()
        {
            var decision = new GameDecision { DecisionId = "T" };
            Session.Insert(decision);
            Session.Insert(new GameAlert { Decision = decision, Severity = 1, Message = "info" });
            Session.Fire();

            Verify(v => v.Rule<ExecuteDecisionRule>().Fired(Times.Once));
            Assert.That(decision.Status, Is.EqualTo(DecisionStatus.Execute));
        }

        [Test]
        public void DoesNotFire_When_Severity2_Warning_Present()
        {
            var decision = new GameDecision { DecisionId = "T" };
            Session.Insert(decision);
            Session.Insert(new GameAlert { Decision = decision, Severity = 2, Message = "warn" });
            Session.Fire();

            Verify(v => v.Rule<ExecuteDecisionRule>().Fired(Times.Never));
        }

        [Test]
        public void DoesNotFire_When_Severity3_Error_Present()
        {
            var decision = new GameDecision { DecisionId = "T" };
            Session.Insert(decision);
            Session.Insert(new GameAlert { Decision = decision, Severity = 3, Message = "err" });
            Session.Fire();

            Verify(v => v.Rule<ExecuteDecisionRule>().Fired(Times.Never));
        }
    }

    [TestFixture]
    public class ReviewDecisionRuleTests : IsolatedRulesTestFixture
    {
        public ReviewDecisionRuleTests()
        {
            Setup.Rule<ReviewDecisionRule>();
        }

        [Test]
        public void Fires_When_Severity2_Alert_Present()
        {
            var decision = new GameDecision { DecisionId = "T" };
            Session.Insert(decision);
            Session.Insert(new GameAlert { Decision = decision, Severity = 2, Message = "warn" });
            Session.Fire();

            Verify(v => v.Rule<ReviewDecisionRule>().Fired(Times.Once));
            Assert.That(decision.Status, Is.EqualTo(DecisionStatus.Review));
        }

        [Test]
        public void DoesNotFire_When_OnlyInfoAlerts()
        {
            var decision = new GameDecision { DecisionId = "T" };
            Session.Insert(decision);
            Session.Insert(new GameAlert { Decision = decision, Severity = 1, Message = "info" });
            Session.Fire();

            Verify(v => v.Rule<ReviewDecisionRule>().Fired(Times.Never));
        }
    }

    [TestFixture]
    public class AbortDecisionRuleTests : IsolatedRulesTestFixture
    {
        public AbortDecisionRuleTests()
        {
            Setup.Rule<AbortDecisionRule>();
        }

        [Test]
        public void Fires_When_Severity3_Error_Present()
        {
            var decision = new GameDecision { DecisionId = "T" };
            Session.Insert(decision);
            Session.Insert(new GameAlert { Decision = decision, Severity = 3, Message = "err" });
            Session.Fire();

            Verify(v => v.Rule<AbortDecisionRule>().Fired(Times.Once));
            Assert.That(decision.Status, Is.EqualTo(DecisionStatus.Abort));
        }

        [Test]
        public void DoesNotFire_When_OnlyWarningAlerts()
        {
            var decision = new GameDecision { DecisionId = "T" };
            Session.Insert(decision);
            Session.Insert(new GameAlert { Decision = decision, Severity = 2, Message = "warn" });
            Session.Fire();

            Verify(v => v.Rule<AbortDecisionRule>().Fired(Times.Never));
        }

        [Test]
        public void DoesNotFire_When_AlertsForDifferentDecision()
        {
            var decisionA = new GameDecision { DecisionId = "A" };
            var decisionB = new GameDecision { DecisionId = "B" };
            Session.Insert(decisionA);
            Session.Insert(decisionB);
            Session.Insert(new GameAlert { Decision = decisionB, Severity = 3, Message = "err for B" });
            Session.Fire();

            Verify(v => v.Rule<AbortDecisionRule>().Fired(Times.Once));
            Assert.That(decisionA.Status, Is.EqualTo(DecisionStatus.Open));
            Assert.That(decisionB.Status, Is.EqualTo(DecisionStatus.Abort));
        }
    }
}