// Перечисления предметной области: роль персонажа, склонность к риску, архетип врага.
namespace ExpertSystem.RuleEngine.Core.Domain
{
    /// <summary>Боевая роль персонажа (задаёт, какие ролевые правила применимы).</summary>
    public enum CharacterRole
    {
        Unspecified = 0,
        Tank = 1,
        Dps = 2,
        Healer = 3,
        Support = 4,
    }

    /// <summary>Склонность персонажа к риску.</summary>
    public enum RiskTolerance
    {
        Balanced = 0,
        Cautious = 1,
        Aggressive = 2,
    }

    /// <summary>Архетип противника.</summary>
    public enum EnemyArchetype
    {
        Unknown = 0,
        Melee = 1,
        Ranged = 2,
        Caster = 3,
        Boss = 4,
        Minion = 5,
    }
}
