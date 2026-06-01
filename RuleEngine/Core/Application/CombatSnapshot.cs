// Плоский входной DTO: все характеристики боевой ситуации в одном объекте.
// Передаётся в сервис вывода; из него строятся доменные факты.
using System.Collections.Generic;
using ExpertSystem.RuleEngine.Core.Domain;

namespace ExpertSystem.RuleEngine.Core.Application
{
    /// <summary>Снимок боевой ситуации с точки зрения одного действующего персонажа.</summary>
    public class CombatSnapshot
    {
        // Идентификаторы решения, актора и цели.
        public string DecisionId { get; set; } = string.Empty;
        public string ActorId { get; set; } = string.Empty;
        public string TargetId { get; set; } = string.Empty;

        // Числовые характеристики состояния боя.
        public int PlayerHealthPercent { get; set; }
        public int PlayerManaPercent { get; set; }
        public int NearbyAllyCount { get; set; }
        public int NearbyEnemyCount { get; set; }
        public int StrongEnemyCount { get; set; }
        public int LowestAllyHealthPercent { get; set; } = 100;

        // Флаги доступных навыков и состояния цели.
        public bool SkillOnCooldown { get; set; }
        public bool HasHealingPotion { get; set; }
        public bool InterruptSkillReady { get; set; }
        public bool EscapeSkillReady { get; set; }
        public bool TargetIsBoss { get; set; }
        public bool TargetIsCastingHighImpactSkill { get; set; }
        public int TargetHealthPercent { get; set; }
        public int TargetThreatPercent { get; set; }
        public bool HasLineOfSight { get; set; } = true;

        // Состояния обездвиживания.
        public bool IsStunned { get; set; }
        public bool IsSilenced { get; set; }
        public bool IsRooted { get; set; }

        // Остатки откатов навыков, секунды.
        public float BurstCooldownSeconds { get; set; }
        public float InterruptCooldownSeconds { get; set; }
        public float EscapeCooldownSeconds { get; set; }

        // Профильные характеристики актора (роль активирует ролевые правила).
        public CharacterRole Role { get; set; } = CharacterRole.Unspecified;
        public RiskTolerance RiskTolerance { get; set; } = RiskTolerance.Balanced;
        public int ActorLevel { get; set; } = 1;

        // Список наблюдаемых противников.
        public List<EnemyProfile> Enemies { get; } = new List<EnemyProfile>();
    }
}
