using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.Rules.CombatRules;
using NRules.Testing;
using NUnit.Framework;

namespace ExpertSystem.RuleEngine.Tests.Editor
{
    [TestFixture]
    public class LowHealthRuleTests : IsolatedRulesTestFixture
    {
        public LowHealthRuleTests() { Setup.Rule<LowHealthRule>(); }

        [Test]
        public void Fires_When_PlayerHealth_AtOrBelow_20()
        {
            var decision = new GameDecision { DecisionId = "T1" };
            var state = new CombatState { PlayerHealthPercent = 20 };

            Session.Insert(decision);
            Session.Insert(state);
            Session.Fire();

            Verify(v => v.Rule<LowHealthRule>().Fired(Times.Once));
        }

        [Test]
        public void DoesNotFire_When_PlayerHealth_Above_20()
        {
            var decision = new GameDecision { DecisionId = "T2" };
            var state = new CombatState { PlayerHealthPercent = 21 };

            Session.Insert(decision);
            Session.Insert(state);
            Session.Fire();

            Verify(v => v.Rule<LowHealthRule>().Fired(Times.Never));
        }
    }

    [TestFixture]
    public class SelfHealRuleTests : IsolatedRulesTestFixture
    {
        public SelfHealRuleTests() { Setup.Rule<SelfHealRule>(); }

        [Test]
        public void Fires_When_LowHealth_And_HasPotion()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState { PlayerHealthPercent = 35, HasHealingPotion = true });
            Session.Fire();

            Verify(v => v.Rule<SelfHealRule>().Fired(Times.Once));
        }

        [Test]
        public void DoesNotFire_When_NoHealingPotion()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState { PlayerHealthPercent = 35, HasHealingPotion = false });
            Session.Fire();

            Verify(v => v.Rule<SelfHealRule>().Fired(Times.Never));
        }

        [Test]
        public void DoesNotFire_When_HealthAboveThreshold()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState { PlayerHealthPercent = 55, HasHealingPotion = true });
            Session.Fire();

            Verify(v => v.Rule<SelfHealRule>().Fired(Times.Never));
        }
    }

    [TestFixture]
    public class EmergencyRetreatRuleTests : IsolatedRulesTestFixture
    {
        public EmergencyRetreatRuleTests() { Setup.Rule<EmergencyRetreatRule>(); }

        [Test]
        public void Fires_When_Cornered_With_LowHp_ManyEnemies_NoEscape()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState
            {
                PlayerHealthPercent = 20,
                NearbyEnemyCount = 4,
                EscapeSkillReady = false,
            });
            Session.Fire();

            Verify(v => v.Rule<EmergencyRetreatRule>().Fired(Times.Once));
        }

        [Test]
        public void DoesNotFire_When_EscapeSkillReady()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState
            {
                PlayerHealthPercent = 15,
                NearbyEnemyCount = 5,
                EscapeSkillReady = true,
            });
            Session.Fire();

            Verify(v => v.Rule<EmergencyRetreatRule>().Fired(Times.Never));
        }
    }

    [TestFixture]
    public class EnemyOverwhelmRuleTests : IsolatedRulesTestFixture
    {
        public EnemyOverwhelmRuleTests() { Setup.Rule<EnemyOverwhelmRule>(); }

        [Test]
        public void Fires_When_ManyEnemies_With_StrongPresence()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState { NearbyEnemyCount = 5, StrongEnemyCount = 2 });
            Session.Fire();

            Verify(v => v.Rule<EnemyOverwhelmRule>().Fired(Times.Once));
        }

        [Test]
        public void DoesNotFire_When_FewEnemies()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState { NearbyEnemyCount = 3, StrongEnemyCount = 2 });
            Session.Fire();

            Verify(v => v.Rule<EnemyOverwhelmRule>().Fired(Times.Never));
        }
    }

    [TestFixture]
    public class FinisherBurstRuleTests : IsolatedRulesTestFixture
    {
        public FinisherBurstRuleTests() { Setup.Rule<FinisherBurstRule>(); }

        [Test]
        public void Fires_When_TargetNearDeath_Mana_And_Clear()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState
            {
                TargetHealthPercent = 20,
                PlayerManaPercent = 50,
                NearbyEnemyCount = 1,
                HasLineOfSight = true,
            });
            Session.Fire();

            Verify(v => v.Rule<FinisherBurstRule>().Fired(Times.Once));
        }

        [Test]
        public void DoesNotFire_When_NoLineOfSight()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState
            {
                TargetHealthPercent = 20,
                PlayerManaPercent = 50,
                NearbyEnemyCount = 1,
                HasLineOfSight = false,
            });
            Session.Fire();

            Verify(v => v.Rule<FinisherBurstRule>().Fired(Times.Never));
        }

        [Test]
        public void DoesNotFire_When_TooManyEnemies()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState
            {
                TargetHealthPercent = 20,
                PlayerManaPercent = 50,
                NearbyEnemyCount = 3,
                HasLineOfSight = true,
            });
            Session.Fire();

            Verify(v => v.Rule<FinisherBurstRule>().Fired(Times.Never));
        }
    }

    [TestFixture]
    public class InterruptCasterRuleTests : IsolatedRulesTestFixture
    {
        public InterruptCasterRuleTests() { Setup.Rule<InterruptCasterRule>(); }

        [Test]
        public void Fires_When_EnemyCastingHighImpact_And_InterruptReady()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState
            {
                InterruptSkillReady = true,
                TargetIsCastingHighImpactSkill = true,
                HasLineOfSight = true,
            });
            Session.Fire();

            Verify(v => v.Rule<InterruptCasterRule>().Fired(Times.Once));
        }

        [Test]
        public void DoesNotFire_When_InterruptOnCooldown()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState
            {
                InterruptSkillReady = false,
                TargetIsCastingHighImpactSkill = true,
                HasLineOfSight = true,
            });
            Session.Fire();

            Verify(v => v.Rule<InterruptCasterRule>().Fired(Times.Never));
        }
    }

    [TestFixture]
    public class ResourcePressureRuleTests : IsolatedRulesTestFixture
    {
        public ResourcePressureRuleTests() { Setup.Rule<ResourcePressureRule>(); }

        [Test]
        public void Fires_When_LowMana()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState { PlayerManaPercent = 20 });
            Session.Fire();

            Verify(v => v.Rule<ResourcePressureRule>().Fired(Times.Once));
        }

        [Test]
        public void Fires_When_SkillOnCooldown_Even_With_Mana()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState { PlayerManaPercent = 90, SkillOnCooldown = true });
            Session.Fire();

            Verify(v => v.Rule<ResourcePressureRule>().Fired(Times.Once));
        }

        [Test]
        public void DoesNotFire_When_FullResources()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState { PlayerManaPercent = 60, SkillOnCooldown = false });
            Session.Fire();

            Verify(v => v.Rule<ResourcePressureRule>().Fired(Times.Never));
        }
    }

    [TestFixture]
    public class FocusHighThreatTargetRuleTests : IsolatedRulesTestFixture
    {
        public FocusHighThreatTargetRuleTests() { Setup.Rule<FocusHighThreatTargetRule>(); }

        [Test]
        public void Fires_When_HighThreat_And_Healthy()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState
            {
                TargetThreatPercent = 80,
                PlayerHealthPercent = 70,
                HasLineOfSight = true,
            });
            Session.Fire();

            Verify(v => v.Rule<FocusHighThreatTargetRule>().Fired(Times.Once));
        }

        [Test]
        public void DoesNotFire_When_PlayerTooLow()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState
            {
                TargetThreatPercent = 80,
                PlayerHealthPercent = 30,
                HasLineOfSight = true,
            });
            Session.Fire();

            Verify(v => v.Rule<FocusHighThreatTargetRule>().Fired(Times.Never));
        }
    }

    [TestFixture]
    public class StunnedHoldRuleTests : IsolatedRulesTestFixture
    {
        public StunnedHoldRuleTests() { Setup.Rule<StunnedHoldRule>(); }

        [Test]
        public void Fires_When_Stunned()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState { IsStunned = true });
            Session.Fire();

            Verify(v => v.Rule<StunnedHoldRule>().Fired(Times.Once));
        }

        [Test]
        public void Fires_When_Silenced()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState { IsSilenced = true });
            Session.Fire();

            Verify(v => v.Rule<StunnedHoldRule>().Fired(Times.Once));
        }

        [Test]
        public void Fires_When_Rooted()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState { IsRooted = true });
            Session.Fire();

            Verify(v => v.Rule<StunnedHoldRule>().Fired(Times.Once));
        }

        [Test]
        public void DoesNotFire_When_Healthy()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState());
            Session.Fire();

            Verify(v => v.Rule<StunnedHoldRule>().Fired(Times.Never));
        }
    }

    [TestFixture]
    public class TankDefendAllyRuleTests : IsolatedRulesTestFixture
    {
        public TankDefendAllyRuleTests() { Setup.Rule<TankDefendAllyRule>(); }

        [Test]
        public void Fires_When_Tank_AllyInDanger_SelfHealthy()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState
            {
                LowestAllyHealthPercent = 35,
                NearbyEnemyCount = 2,
                PlayerHealthPercent = 70,
            });
            Session.Insert(new CharacterProfile { Role = CharacterRole.Tank });
            Session.Fire();

            Verify(v => v.Rule<TankDefendAllyRule>().Fired(Times.Once));
        }

        [Test]
        public void DoesNotFire_When_NotTank()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState
            {
                LowestAllyHealthPercent = 20,
                NearbyEnemyCount = 2,
                PlayerHealthPercent = 70,
            });
            Session.Insert(new CharacterProfile { Role = CharacterRole.Dps });
            Session.Fire();

            Verify(v => v.Rule<TankDefendAllyRule>().Fired(Times.Never));
        }

        [Test]
        public void DoesNotFire_When_AllyFull()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState
            {
                LowestAllyHealthPercent = 95,
                NearbyEnemyCount = 2,
                PlayerHealthPercent = 70,
            });
            Session.Insert(new CharacterProfile { Role = CharacterRole.Tank });
            Session.Fire();

            Verify(v => v.Rule<TankDefendAllyRule>().Fired(Times.Never));
        }
    }

    [TestFixture]
    public class HealerAllyTriageRuleTests : IsolatedRulesTestFixture
    {
        public HealerAllyTriageRuleTests() { Setup.Rule<HealerAllyTriageRule>(); }

        [Test]
        public void Fires_When_Healer_With_CriticallyInjuredAlly()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState { LowestAllyHealthPercent = 20, PlayerManaPercent = 60 });
            Session.Insert(new CharacterProfile { Role = CharacterRole.Healer });
            Session.Fire();

            Verify(v => v.Rule<HealerAllyTriageRule>().Fired(Times.Once));
        }

        [Test]
        public void DoesNotFire_When_OutOfMana()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState { LowestAllyHealthPercent = 20, PlayerManaPercent = 10 });
            Session.Insert(new CharacterProfile { Role = CharacterRole.Healer });
            Session.Fire();

            Verify(v => v.Rule<HealerAllyTriageRule>().Fired(Times.Never));
        }
    }

    [TestFixture]
    public class MultiCasterAoeInterruptRuleTests : IsolatedRulesTestFixture
    {
        public MultiCasterAoeInterruptRuleTests() { Setup.Rule<MultiCasterAoeInterruptRule>(); }

        [Test]
        public void Fires_When_TwoOrMore_EnemiesCasting()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState { InterruptSkillReady = true });
            Session.Insert(new EnemyProfile { EnemyId = "E1", IsCasting = true });
            Session.Insert(new EnemyProfile { EnemyId = "E2", IsCasting = true });
            Session.Fire();

            Verify(v => v.Rule<MultiCasterAoeInterruptRule>().Fired(Times.Once));
        }

        [Test]
        public void DoesNotFire_When_OnlyOneCaster()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState { InterruptSkillReady = true });
            Session.Insert(new EnemyProfile { EnemyId = "E1", IsCasting = true });
            Session.Insert(new EnemyProfile { EnemyId = "E2", IsCasting = false });
            Session.Fire();

            Verify(v => v.Rule<MultiCasterAoeInterruptRule>().Fired(Times.Never));
        }

        [Test]
        public void DoesNotFire_When_InterruptOnCooldown()
        {
            Session.Insert(new GameDecision { DecisionId = "T" });
            Session.Insert(new CombatState { InterruptSkillReady = false });
            Session.Insert(new EnemyProfile { EnemyId = "E1", IsCasting = true });
            Session.Insert(new EnemyProfile { EnemyId = "E2", IsCasting = true });
            Session.Fire();

            Verify(v => v.Rule<MultiCasterAoeInterruptRule>().Fired(Times.Never));
        }
    }
}
