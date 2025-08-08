using UnityEngine;
using System.Collections.Generic;

public static class TowerShotBuilder
{
    public static ShotData BuildShotData(Tower tower, TowerEffects towerEffects)
    {
        ShotData shot = new ShotData();

        // Projectiles
        if (towerEffects != null && towerEffects.useProjectile)
        {
            shot.projectilePrefab = towerEffects.projectilePrefab;
        }
        else
        {
            shot.projectilePrefab = null; // direct hit fallback
        }

        // Read rule multipliers (safe defaults)
        var towerRule = RuleManager.main != null ? RuleManager.main.GetCurrentlyAppliedTowerRule() : null;

        float damageMul = towerRule != null ? towerRule.damageMultiplier : 1f;
        float projectileSpeedMul = towerRule != null ? towerRule.projectileSpeedMultiplier : 1f;
        float aoeMul = towerRule != null ? towerRule.aoeRadiusMultiplier : 1f;
        float slowMul = towerRule != null ? towerRule.slowEffectMultiplier : 1f;
        float dotMul = towerRule != null ? towerRule.dotDamageMultiplier : 1f;
        float stunDurMul = towerRule != null ? towerRule.stunDurationMultiplier : 1f;

        // Damage snapshot (tower.Damage već uključuje upgrade promjene)
        shot.damage = Mathf.CeilToInt(tower.Damage * damageMul);

        // Projectile speed snapshot
        // Projectile speed snapshot (TowerUpgrade može modificirati base projectileSpeed)
        float baseProjSpeed = towerEffects != null ? towerEffects.projectileSpeed : 0f;
        shot.projectileSpeed = baseProjSpeed * projectileSpeedMul;

        // Effects snapshot (copied and scaled)
        shot.effects = new List<TowerEffect>();
        if (towerEffects != null && towerEffects.effects != null)
        {
            foreach (var e in towerEffects.effects)
            {
                if (e == null) continue;
                var copy = new TowerEffect
                {
                    effectType = e.effectType,
                    effectPrefab = e.effectPrefab,
                    effectColor = e.effectColor,
                    // Base effect values već uključuju upgrade progresije (TowerUpgrade ih je mutirao)
                    effectRadius = e.effectRadius * aoeMul,
                    effectDuration = e.effectDuration * ((e.effectType == EffectType.Stun || e.effectType == EffectType.AOE_Stun) ? stunDurMul : 1f),
                    effectStrength = e.effectStrength * ((e.effectType == EffectType.Slow || e.effectType == EffectType.AOE_Slow) ? slowMul : 1f),
                    dotDamage = Mathf.CeilToInt(e.dotDamage * ((e.effectType == EffectType.DOT || e.effectType == EffectType.DOT_AOE) ? dotMul : 1f)),
                };
                shot.effects.Add(copy);
            }
        }

        return shot;
    }
}
