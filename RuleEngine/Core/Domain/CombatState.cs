// Факт состояния боя — основной входной факт рабочей памяти. Сеттеры ограничивают
// значения допустимыми диапазонами, защищая правила от некорректных данных.
namespace ExpertSystem.RuleEngine.Core.Domain
{
    /// <summary>Текущее состояние боя глазами действующего персонажа.</summary>
    public class CombatState
    {
        private int _playerHealthPercent;
        private int _playerManaPercent;
        private int _targetHealthPercent;
        private int _targetThreatPercent;
        private int _nearbyAllyCount;
        private int _nearbyEnemyCount;
        private int _strongEnemyCount;
        private int _lowestAllyHealthPercent = 100;
        private float _burstCooldownSeconds;
        private float _interruptCooldownSeconds;
        private float _escapeCooldownSeconds;

        // Проценты ограничиваются диапазоном [0, 100].
        public int PlayerHealthPercent
        {
            get => _playerHealthPercent;
            set => _playerHealthPercent = ClampPercent(value);
        }

        public int PlayerManaPercent
        {
            get => _playerManaPercent;
            set => _playerManaPercent = ClampPercent(value);
        }

        // Счётчики не могут быть отрицательными.
        public int NearbyAllyCount
        {
            get => _nearbyAllyCount;
            set => _nearbyAllyCount = ClampNonNegative(value);
        }

        public int NearbyEnemyCount
        {
            get => _nearbyEnemyCount;
            set => _nearbyEnemyCount = ClampNonNegative(value);
        }

        public int StrongEnemyCount
        {
            get => _strongEnemyCount;
            set => _strongEnemyCount = ClampNonNegative(value);
        }

        public int LowestAllyHealthPercent
        {
            get => _lowestAllyHealthPercent;
            set => _lowestAllyHealthPercent = ClampPercent(value);
        }

        public bool SkillOnCooldown { get; set; }
        public bool HasHealingPotion { get; set; }
        public bool InterruptSkillReady { get; set; }
        public bool EscapeSkillReady { get; set; }
        public bool TargetIsBoss { get; set; }
        public bool TargetIsCastingHighImpactSkill { get; set; }

        public bool IsStunned { get; set; }
        public bool IsSilenced { get; set; }
        public bool IsRooted { get; set; }

        public float BurstCooldownSeconds
        {
            get => _burstCooldownSeconds;
            set => _burstCooldownSeconds = ClampNonNegative(value);
        }

        public float InterruptCooldownSeconds
        {
            get => _interruptCooldownSeconds;
            set => _interruptCooldownSeconds = ClampNonNegative(value);
        }

        public float EscapeCooldownSeconds
        {
            get => _escapeCooldownSeconds;
            set => _escapeCooldownSeconds = ClampNonNegative(value);
        }

        /// <summary>Признак "персонаж не может действовать" — любое из состояний обездвиживания.</summary>
        public bool IsIncapacitated => IsStunned || IsSilenced || IsRooted;

        public int TargetHealthPercent
        {
            get => _targetHealthPercent;
            set => _targetHealthPercent = ClampPercent(value);
        }

        public int TargetThreatPercent
        {
            get => _targetThreatPercent;
            set => _targetThreatPercent = ClampPercent(value);
        }

        public bool HasLineOfSight { get; set; } = true;

        /// <summary>Ограничивает значение диапазоном [0, 100].</summary>
        private static int ClampPercent(int value)
        {
            if (value < 0) return 0;
            if (value > 100) return 100;
            return value;
        }

        /// <summary>Обрезает отрицательные целые до нуля.</summary>
        private static int ClampNonNegative(int value)
        {
            return value < 0 ? 0 : value;
        }

        /// <summary>Обрезает отрицательные дробные до нуля.</summary>
        private static float ClampNonNegative(float value)
        {
            return value < 0f ? 0f : value;
        }
    }
}
