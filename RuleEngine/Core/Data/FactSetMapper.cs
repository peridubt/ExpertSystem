using ExpertSystem.RuleEngine.Core.Application;
using ExpertSystem.RuleEngine.Core.Domain;

namespace ExpertSystem.RuleEngine.Core.Data
{
    public static class FactSetMapper
    {
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
