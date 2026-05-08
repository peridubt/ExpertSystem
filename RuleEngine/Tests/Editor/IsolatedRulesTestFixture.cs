using System;
using System.Reflection;
using NRules.Testing;
using NUnit.Framework;

namespace ExpertSystem.RuleEngine.Tests.Editor
{
    public abstract class IsolatedRulesTestFixture : RulesTestFixture
    {
        private static readonly FieldInfo HarnessField =
            typeof(RulesTestFixture).GetField(
                "_testHarness",
                BindingFlags.Instance | BindingFlags.NonPublic)!;

        [SetUp]
        public void ResetHarnessPerTest()
        {
            var fresh = new Lazy<RulesTestHarness>(() => new RulesTestHarness(Setup));
            HarnessField.SetValue(this, fresh);
        }
    }
}
