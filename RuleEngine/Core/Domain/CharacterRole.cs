namespace ExpertSystem.RuleEngine.Core.Domain
{
    public enum CharacterRole
    {
        Unspecified = 0,
        Tank = 1,
        Dps = 2,
        Healer = 3,
        Support = 4,
    }

    public enum RiskTolerance
    {
        Balanced = 0,
        Cautious = 1,
        Aggressive = 2,
    }

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
