// Набор эталонных боевых сценариев для проверки корректности движка.
using System.Collections.Generic;
using ExpertSystem.RuleEngine.Core.Application;
using ExpertSystem.RuleEngine.Core.Domain;

namespace ExpertSystem.RuleEngine.Core.Testing
{
    /// <summary>Фабрика стандартных сценариев "ситуация → ожидаемое решение".</summary>
    public static class DefaultRpgScenarios
    {
        /// <summary>Создаёт список эталонных сценариев. Возвращает перечень DecisionScenario.</summary>
        public static IReadOnlyList<DecisionScenario> Create()
        {
            return new List<DecisionScenario>
            {
                // Критическое давление: мало HP, окружён, нет отступления → Retreat + Abort.
                new DecisionScenario
                {
                    Name = "Retreat under lethal pressure",
                    ExpectedStatus = DecisionStatus.Abort,
                    ExpectedAction = TacticalAction.Retreat,
                    ExpectedRule = "Emergency retreat",
                    Snapshot = new CombatSnapshot
                    {
                        DecisionId = "S1",
                        ActorId = "Player",
                        TargetId = "EnemyBoss",
                        PlayerHealthPercent = 20,
                        PlayerManaPercent = 40,
                        NearbyEnemyCount = 4,
                        StrongEnemyCount = 2,
                        EscapeSkillReady = false,
                        HasLineOfSight = true,
                    },
                },
                // Опасный каст врага + готов прерыватель → Interrupt + Review.
                new DecisionScenario
                {
                    Name = "Interrupt dangerous cast",
                    ExpectedStatus = DecisionStatus.Review,
                    ExpectedAction = TacticalAction.Interrupt,
                    ExpectedRule = "Interrupt dangerous cast",
                    Snapshot = new CombatSnapshot
                    {
                        DecisionId = "S2",
                        ActorId = "Player",
                        TargetId = "EnemyMage",
                        PlayerHealthPercent = 75,
                        PlayerManaPercent = 60,
                        NearbyEnemyCount = 1,
                        InterruptSkillReady = true,
                        TargetIsCastingHighImpactSkill = true,
                        HasLineOfSight = true,
                    },
                },
                // Чистое окно на добивание цели → Burst + Execute.
                new DecisionScenario
                {
                    Name = "Clean execute window",
                    ExpectedStatus = DecisionStatus.Execute,
                    ExpectedAction = TacticalAction.Burst,
                    ExpectedRule = "Finisher burst",
                    Snapshot = new CombatSnapshot
                    {
                        DecisionId = "S3",
                        ActorId = "Player",
                        TargetId = "EnemyRogue",
                        PlayerHealthPercent = 80,
                        PlayerManaPercent = 70,
                        NearbyEnemyCount = 1,
                        TargetHealthPercent = 20,
                        HasLineOfSight = true,
                    },
                },
            };
        }
    }
}
