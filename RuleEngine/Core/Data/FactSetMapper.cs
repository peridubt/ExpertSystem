// Преобразования между сериализуемыми DTO и доменными/прикладными объектами.
// Нужен отдельный слой, т.к. JsonUtility работает с полями, а домен — со свойствами.
using ExpertSystem.RuleEngine.Core.Application;
using ExpertSystem.RuleEngine.Core.Domain;

namespace ExpertSystem.RuleEngine.Core.Data
{
    /// <summary>Маппер "DTO ↔ снимок/результат".</summary>
    public static class FactSetMapper
    {
        /// <summary>
        /// Строит снимок боя из набора фактов. Принимает DTO набора фактов,
        /// возвращает CombatSnapshot (или null, если данных нет).
        /// </summary>
        public static CombatSnapshot ToSnapshot(FactSetData data)
        {
            if (data == null) return null;

            var snap = new CombatSnapshot
            {
                DecisionId = data.decisionId,
                ActorId = data.actorId,
                TargetId = data.targetId,
            };

            var cs = data.combatState ?? new CombatStateData();
            snap.PlayerHealthPercent = cs.playerHealthPercent;
            snap.PlayerManaPercent = cs.playerManaPercent;
            snap.TargetHealthPercent = cs.targetHealthPercent;
            snap.TargetThreatPercent = cs.targetThreatPercent;
            snap.NearbyAllyCount = cs.nearbyAllyCount;
            snap.NearbyEnemyCount = cs.nearbyEnemyCount;
            snap.StrongEnemyCount = cs.strongEnemyCount;
            snap.LowestAllyHealthPercent = cs.lowestAllyHealthPercent;
            snap.SkillOnCooldown = cs.skillOnCooldown;
            snap.HasHealingPotion = cs.hasHealingPotion;
            snap.InterruptSkillReady = cs.interruptSkillReady;
            snap.EscapeSkillReady = cs.escapeSkillReady;
            snap.TargetIsBoss = cs.targetIsBoss;
            snap.TargetIsCastingHighImpactSkill = cs.targetIsCastingHighImpactSkill;
            snap.IsStunned = cs.isStunned;
            snap.IsSilenced = cs.isSilenced;
            snap.IsRooted = cs.isRooted;
            snap.HasLineOfSight = cs.hasLineOfSight;
            snap.BurstCooldownSeconds = cs.burstCooldownSeconds;
            snap.InterruptCooldownSeconds = cs.interruptCooldownSeconds;
            snap.EscapeCooldownSeconds = cs.escapeCooldownSeconds;

            // Профиль переносим только при заданной роли — иначе ролевые правила не нужны.
            if (data.profile != null && data.profile.role != CharacterRole.Unspecified)
            {
                snap.Role = data.profile.role;
                snap.RiskTolerance = data.profile.riskTolerance;
                snap.ActorLevel = data.profile.level;
            }

            if (data.enemies != null)
            {
                foreach (var e in data.enemies)
                {
                    if (e == null) continue;
                    snap.Enemies.Add(new EnemyProfile
                    {
                        EnemyId = e.enemyId,
                        Archetype = e.archetype,
                        HealthPercent = e.healthPercent,
                        ThreatPercent = e.threatPercent,
                        DistanceMeters = e.distanceMeters,
                        IsCasting = e.isCasting,
                        IsElite = e.isElite,
                    });
                }
            }

            return snap;
        }

        /// <summary>
        /// Строит набор фактов из снимка (для сохранения изменённого состояния обратно
        /// в JSON). Принимает снимок и идентификатор набора, возвращает FactSetData.
        /// </summary>
        public static FactSetData ToData(CombatSnapshot snapshot, string factSetId)
        {
            if (snapshot == null) return null;

            var data = new FactSetData
            {
                factSetId = factSetId ?? string.Empty,
                decisionId = snapshot.DecisionId,
                actorId = snapshot.ActorId,
                targetId = snapshot.TargetId,
                combatState = new CombatStateData
                {
                    playerHealthPercent = snapshot.PlayerHealthPercent,
                    playerManaPercent = snapshot.PlayerManaPercent,
                    targetHealthPercent = snapshot.TargetHealthPercent,
                    targetThreatPercent = snapshot.TargetThreatPercent,
                    nearbyAllyCount = snapshot.NearbyAllyCount,
                    nearbyEnemyCount = snapshot.NearbyEnemyCount,
                    strongEnemyCount = snapshot.StrongEnemyCount,
                    lowestAllyHealthPercent = snapshot.LowestAllyHealthPercent,
                    skillOnCooldown = snapshot.SkillOnCooldown,
                    hasHealingPotion = snapshot.HasHealingPotion,
                    interruptSkillReady = snapshot.InterruptSkillReady,
                    escapeSkillReady = snapshot.EscapeSkillReady,
                    targetIsBoss = snapshot.TargetIsBoss,
                    targetIsCastingHighImpactSkill = snapshot.TargetIsCastingHighImpactSkill,
                    isStunned = snapshot.IsStunned,
                    isSilenced = snapshot.IsSilenced,
                    isRooted = snapshot.IsRooted,
                    hasLineOfSight = snapshot.HasLineOfSight,
                    burstCooldownSeconds = snapshot.BurstCooldownSeconds,
                    interruptCooldownSeconds = snapshot.InterruptCooldownSeconds,
                    escapeCooldownSeconds = snapshot.EscapeCooldownSeconds,
                },
                profile = new CharacterProfileData
                {
                    actorId = snapshot.ActorId,
                    role = snapshot.Role,
                    riskTolerance = snapshot.RiskTolerance,
                    level = snapshot.ActorLevel,
                },
            };

            if (snapshot.Enemies != null)
            {
                foreach (var e in snapshot.Enemies)
                {
                    if (e == null) continue;
                    data.enemies.Add(new EnemyProfileData
                    {
                        enemyId = e.EnemyId,
                        archetype = e.Archetype,
                        healthPercent = e.HealthPercent,
                        threatPercent = e.ThreatPercent,
                        distanceMeters = e.DistanceMeters,
                        isCasting = e.IsCasting,
                        isElite = e.IsElite,
                    });
                }
            }

            return data;
        }

        /// <summary>
        /// Преобразует результат вывода в DTO для сохранения. Принимает результат и
        /// идентификатор исходного набора фактов, возвращает DecisionResultData.
        /// </summary>
        public static DecisionResultData ToDataResult(DecisionResult result, string sourceFactSetId)
        {
            var data = new DecisionResultData
            {
                evaluatedAtUtc = System.DateTime.UtcNow.ToString("O"),
                sourceFactSetId = sourceFactSetId ?? string.Empty,
                decisionId = result.DecisionId,
                actorId = result.ActorId,
                targetId = result.TargetId,
                status = result.Status,
                suggestedAction = result.SuggestedAction,
                utilityScore = result.UtilityScore,
                tacticalReason = result.TacticalReason,
            };

            foreach (var r in result.TriggeredRules) data.triggeredRules.Add(r);
            foreach (var a in result.Alerts)
            {
                data.alerts.Add(new AlertResultData
                {
                    severity = a.Severity,
                    ruleName = a.RuleName,
                    message = a.Message,
                });
            }

            return data;
        }
    }
}
