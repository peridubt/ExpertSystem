using System;
using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.RuleEngine.Core.Rules;
using ExpertSystem.Rules.NotificationRules;
using NRules.Testing;
using NUnit.Framework;

namespace ExpertSystem.RuleEngine.Tests.Editor
{
    [TestFixture]
    public class AbortedDecisionNotificationRuleTests : IsolatedRulesTestFixture
    {
        private RecordingNotificationService _notifications;

        public AbortedDecisionNotificationRuleTests()
        {
            Setup.Rule<AbortedDecisionNotificationRule>();
        }

        [SetUp]
        public void ConfigureDependencies()
        {
            _notifications = new RecordingNotificationService();
            Session.DependencyResolver = new TestDependencyResolver(_notifications);
        }

        [Test]
        public void Fires_When_Decision_Is_Aborted_And_Notifies()
        {
            var decision = new GameDecision { DecisionId = "T", Status = DecisionStatus.Abort };
            Session.Insert(decision);
            Session.Fire();

            Verify(v => v.Rule<AbortedDecisionNotificationRule>().Fired(Times.Once));
            Assert.That(_notifications.AbortCount, Is.EqualTo(1));
            Assert.That(_notifications.LastDecisionId, Is.EqualTo("T"));
        }

        [Test]
        public void DoesNotFire_When_Decision_Is_Open()
        {
            var decision = new GameDecision { DecisionId = "T", Status = DecisionStatus.Open };
            Session.Insert(decision);
            Session.Fire();

            Verify(v => v.Rule<AbortedDecisionNotificationRule>().Fired(Times.Never));
            Assert.That(_notifications.AbortCount, Is.EqualTo(0));
        }

        [Test]
        public void DoesNotFire_When_Decision_Is_Execute()
        {
            var decision = new GameDecision { DecisionId = "T", Status = DecisionStatus.Execute };
            Session.Insert(decision);
            Session.Fire();

            Verify(v => v.Rule<AbortedDecisionNotificationRule>().Fired(Times.Never));
        }

        private sealed class RecordingNotificationService : INotificationService
        {
            public int AbortCount { get; private set; }
            public string LastDecisionId { get; private set; } = string.Empty;

            public void DecisionAborted(GameDecision decision)
            {
                AbortCount++;
                LastDecisionId = decision.DecisionId;
            }
        }

        private sealed class TestDependencyResolver : NRules.Extensibility.IDependencyResolver
        {
            private readonly INotificationService _notificationService;

            public TestDependencyResolver(INotificationService notificationService)
            {
                _notificationService = notificationService;
            }

            public object Resolve(NRules.Extensibility.IResolutionContext context, Type serviceType)
            {
                if (serviceType == typeof(INotificationService)) return _notificationService;
                throw new InvalidOperationException($"Unknown dependency: {serviceType}");
            }
        }
    }
}