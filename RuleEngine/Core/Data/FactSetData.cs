// Сериализуемые DTO для хранения наборов фактов и результатов вывода в JSON.
// Поля публичные (требование JsonUtility), зеркалят доменные классы.
using System;
using System.Collections.Generic;
using ExpertSystem.RuleEngine.Core.Domain;

namespace ExpertSystem.RuleEngine.Core.Data
{
    /// <summary>Набор фактов: исходные данные для одного прогона движка.</summary>
    [Serializable]
    public class FactSetData
    {
        public int schemaVersion = 1;
        public string factSetId = string.Empty;
        public string description = string.Empty;
        public string updatedAtUtc = string.Empty;

        public string decisionId = string.Empty;
        public string actorId = "Player";
        public string targetId = string.Empty;

        public CombatStateData combatState = new CombatStateData();
        public CharacterProfileData profile = new CharacterProfileData();
        public List<EnemyProfileData> enemies = new List<EnemyProfileData>();
    }

    /// <summary>DTO состояния боя.</summary>
    [Serializable]
    public class CombatStateData
    {
        public int playerHealthPercent = 100;
        public int playerManaPercent = 100;
        public int targetHealthPercent = 100;
        public int targetThreatPercent = 0;
        public int nearbyAllyCount = 0;
        public int nearbyEnemyCount = 0;
        public int strongEnemyCount = 0;
        public int lowestAllyHealthPercent = 100;
        public bool skillOnCooldown;
        public bool hasHealingPotion;
        public bool interruptSkillReady;
        public bool escapeSkillReady;
        public bool targetIsBoss;
        public bool targetIsCastingHighImpactSkill;
        public bool isStunned;
        public bool isSilenced;
        public bool isRooted;
        public bool hasLineOfSight = true;
        public float burstCooldownSeconds;
        public float interruptCooldownSeconds;
        public float escapeCooldownSeconds;
    }

    /// <summary>DTO профиля персонажа.</summary>
    [Serializable]
    public class CharacterProfileData
    {
        public string actorId = string.Empty;
        public CharacterRole role = CharacterRole.Unspecified;
        public RiskTolerance riskTolerance = RiskTolerance.Balanced;
        public int level = 1;
    }

    /// <summary>DTO противника.</summary>
    [Serializable]
    public class EnemyProfileData
    {
        public string enemyId = string.Empty;
        public EnemyArchetype archetype = EnemyArchetype.Unknown;
        public int healthPercent = 100;
        public int threatPercent;
        public float distanceMeters;
        public bool isCasting;
        public bool isElite;
    }

    /// <summary>DTO результата вывода для сохранения в JSON.</summary>
    [Serializable]
    public class DecisionResultData
    {
        public int schemaVersion = 1;
        public string evaluatedAtUtc = string.Empty;
        public string sourceFactSetId = string.Empty;

        public string decisionId = string.Empty;
        public string actorId = string.Empty;
        public string targetId = string.Empty;
        public DecisionStatus status;
        public TacticalAction suggestedAction;
        public int utilityScore;
        public string tacticalReason = string.Empty;
        public List<string> triggeredRules = new List<string>();
        public List<AlertResultData> alerts = new List<AlertResultData>();
    }

    /// <summary>DTO оповещения в составе результата.</summary>
    [Serializable]
    public class AlertResultData
    {
        public int severity;
        public string ruleName = string.Empty;
        public string message = string.Empty;
    }
}
