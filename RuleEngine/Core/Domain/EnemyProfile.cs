// Факт-описание одного противника боевой ситуации.
namespace ExpertSystem.RuleEngine.Core.Domain
{
    /// <summary>Характеристики отдельного врага.</summary>
    public class EnemyProfile
    {
        public string EnemyId { get; set; } = string.Empty;
        public EnemyArchetype Archetype { get; set; } = EnemyArchetype.Unknown;
        public int HealthPercent { get; set; } = 100;
        public int ThreatPercent { get; set; }
        public float DistanceMeters { get; set; }
        public bool IsCasting { get; set; }
        public bool IsElite { get; set; }
    }
}
