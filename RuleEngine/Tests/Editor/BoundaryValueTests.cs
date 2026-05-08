using ExpertSystem.RuleEngine.Core.Domain;
using ExpertSystem.Rules.CombatRules;
using NRules.Testing;
using NUnit.Framework;

namespace ExpertSystem.RuleEngine.Tests.Editor
{
    /// <summary>
    /// Boundary-value tests: each rule threshold is hit exactly at the edge
    /// and one step beyond, to catch off-by-one errors in rule predicates.
    /// </summary>
    [TestFixture]
    public class SelfHealBoundaryTests : IsolatedRulesTestFixture
    {
        public SelfHealBoundaryTests()
        {
            Setup.Rule<SelfHealRule>();
        }

        [TestCase(40, true)]
        [TestCase(41, false)]
        public void PlayerHealthThreshold_40(int hp, bool shouldFire)
        {
            Session.Insert(new GameDecision());
            Session.Insert(new CombatState { PlayerHealthPercent = hp, HasHealingPotion = true });
            Session.Fire();

            Verify(v => v.Rule<SelfHealRule>().Fired(shouldFire ? Times.Once : Times.Never));
        }
    }

    [TestFixture]
    public class EmergencyRetreatBoundaryTests : IsolatedRulesTestFixture
    {
        public EmergencyRetreatBoundaryTests()
        {
            Setup.Rule<EmergencyRetreatRule>();
        }

        [TestCase(25, 3, true)]
        [TestCase(26, 3, false)]
        [TestCase(25, 2, false)]
        public void HealthAndEnemyCountEdges(int hp, int enemies, bool shouldFire)
        {
            Session.Insert(new GameDecision());
            Session.Insert(new CombatState
            {
                PlayerHealthPercent = hp,
                NearbyEnemyCount = enemies,
                EscapeSkillReady = false,
            });
            Session.Fire();

            Verify(v => v.Rule<EmergencyRetreatRule>().Fired(shouldFire ? Times.Once : Times.Never));
        }
    }

    [TestFixture]
    public class EnemyOverwhelmBoundaryTests : IsolatedRulesTestFixture
    {
        public EnemyOverwhelmBoundaryTests()
        {
            Setup.Rule<EnemyOverwhelmRule>();
        }

        [TestCase(4, 2, true)]
        [TestCase(3, 2, false)]
        [TestCase(4, 1, false)]
        public void EnemyCountEdges(int nearby, int strong, bool shouldFire)
        {
            Session.Insert(new GameDecision());
            Session.Insert(new CombatState { NearbyEnemyCount = nearby, StrongEnemyCount = strong });
            Session.Fire();

            Verify(v => v.Rule<EnemyOverwhelmRule>().Fired(shouldFire ? Times.Once : Times.Never));
        }
    }

    [TestFixture]
    public class FinisherBurstBoundaryTests : IsolatedRulesTestFixture
    {
        public FinisherBurstBoundaryTests()
        {
            Setup.Rule<FinisherBurstRule>();
        }

        [TestCase(25, 30, 2, true)]
        [TestCase(26, 30, 2, false)]
        [TestCase(25, 29, 2, false)]
        [TestCase(25, 30, 3, false)]
        public void ThresholdEdges(int targetHp, int mana, int nearbyEnemies, bool shouldFire)
        {
            Session.Insert(new GameDecision());
            Session.Insert(new CombatState
            {
                TargetHealthPercent = targetHp,
                PlayerManaPercent = mana,
                NearbyEnemyCount = nearbyEnemies,
                HasLineOfSight = true,
            });
            Session.Fire();

            Verify(v => v.Rule<FinisherBurstRule>().Fired(shouldFire ? Times.Once : Times.Never));
        }
    }

    [TestFixture]
    public class FocusHighThreatBoundaryTests : IsolatedRulesTestFixture
    {
        public FocusHighThreatBoundaryTests()
        {
            Setup.Rule<FocusHighThreatTargetRule>();
        }

        [TestCase(70, 36, true)]
        [TestCase(69, 36, false)]
        [TestCase(70, 35, false)]
        public void ThreatAndHealthEdges(int threat, int hp, bool shouldFire)
        {
            Session.Insert(new GameDecision());
            Session.Insert(new CombatState
            {
                TargetThreatPercent = threat,
                PlayerHealthPercent = hp,
                HasLineOfSight = true,
            });
            Session.Fire();

            Verify(v => v.Rule<FocusHighThreatTargetRule>().Fired(shouldFire ? Times.Once : Times.Never));
        }
    }

    [TestFixture]
    public class ResourcePressureBoundaryTests : IsolatedRulesTestFixture
    {
        public ResourcePressureBoundaryTests()
        {
            Setup.Rule<ResourcePressureRule>();
        }

        [TestCase(24, false, true)]
        [TestCase(25, false, false)]
        [TestCase(90, true, true)]
        public void ManaThreshold_25(int mana, bool skillOnCd, bool shouldFire)
        {
            Session.Insert(new GameDecision());
            Session.Insert(new CombatState { PlayerManaPercent = mana, SkillOnCooldown = skillOnCd });
            Session.Fire();

            Verify(v => v.Rule<ResourcePressureRule>().Fired(shouldFire ? Times.Once : Times.Never));
        }
    }

    [TestFixture]
    public class TankDefendAllyBoundaryTests : IsolatedRulesTestFixture
    {
        public TankDefendAllyBoundaryTests()
        {
            Setup.Rule<TankDefendAllyRule>();
        }

        [TestCase(40, 30, true)]
        [TestCase(41, 30, false)]
        [TestCase(40, 29, false)]
        public void AllyHpAndSelfHpEdges(int allyHp, int selfHp, bool shouldFire)
        {
            Session.Insert(new GameDecision());
            Session.Insert(new CombatState
            {
                LowestAllyHealthPercent = allyHp,
                NearbyEnemyCount = 1,
                PlayerHealthPercent = selfHp,
            });
            Session.Insert(new CharacterProfile { Role = CharacterRole.Tank });
            Session.Fire();

            Verify(v => v.Rule<TankDefendAllyRule>().Fired(shouldFire ? Times.Once : Times.Never));
        }
    }

    [TestFixture]
    public class HealerAllyTriageBoundaryTests : IsolatedRulesTestFixture
    {
        public HealerAllyTriageBoundaryTests()
        {
            Setup.Rule<HealerAllyTriageRule>();
        }

        [TestCase(30, 15, true)]
        [TestCase(31, 15, false)]
        [TestCase(30, 14, false)]
        public void AllyHpAndManaEdges(int allyHp, int mana, bool shouldFire)
        {
            Session.Insert(new GameDecision());
            Session.Insert(new CombatState { LowestAllyHealthPercent = allyHp, PlayerManaPercent = mana });
            Session.Insert(new CharacterProfile { Role = CharacterRole.Healer });
            Session.Fire();

            Verify(v => v.Rule<HealerAllyTriageRule>().Fired(shouldFire ? Times.Once : Times.Never));
        }
    }

    [TestFixture]
    public class MultiCasterCountBoundaryTests : IsolatedRulesTestFixture
    {
        public MultiCasterCountBoundaryTests()
        {
            Setup.Rule<MultiCasterAoeInterruptRule>();
        }

        [Test]
        public void ExactlyTwo_Casters_Fires()
        {
            Session.Insert(new GameDecision());
            Session.Insert(new CombatState { InterruptSkillReady = true });
            Session.Insert(new EnemyProfile { EnemyId = "E1", IsCasting = true });
            Session.Insert(new EnemyProfile { EnemyId = "E2", IsCasting = true });
            Session.Fire();

            Verify(v => v.Rule<MultiCasterAoeInterruptRule>().Fired(Times.Once));
        }

        [Test]
        public void Zero_Casters_DoesNotFire()
        {
            Session.Insert(new GameDecision());
            Session.Insert(new CombatState { InterruptSkillReady = true });
            Session.Fire();

            Verify(v => v.Rule<MultiCasterAoeInterruptRule>().Fired(Times.Never));
        }
    }
}