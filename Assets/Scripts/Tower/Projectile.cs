using UnityEngine;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    private ShotData shot;
    private GameObject target;
    private Vector3 lastKnownTargetPos;

    [Header("Visuals")]
    public GameObject explosionEffect;
    public TrailRenderer trail;

    private const float maxLifeSeconds = 5f;

    public void Initialize(ShotData shotData, GameObject targetEnemy)
    {
        shot = shotData;
        target = targetEnemy;

        if (target != null)
        {
            lastKnownTargetPos = target.transform.position;

            Vector3 dir = (lastKnownTargetPos - transform.position).normalized;
            transform.right = dir;
        }

        Destroy(gameObject, maxLifeSeconds);
    }

    void Update()
    {
        if (target != null)
        {
            lastKnownTargetPos = target.transform.position;
        }

        Vector3 direction = (lastKnownTargetPos - transform.position).normalized;
        transform.position += direction * shot.projectileSpeed * Time.deltaTime;
        transform.right = direction;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        var enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;

        // Apply damage
        enemy.TakeDamage(shot.damage);

        // Apply effects
        ApplyShotEffects(shot.effects, enemy.gameObject, other.transform.position);

        // Explosion VFX
        if (explosionEffect != null)
        {
            var fx = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }

        Destroy(gameObject);
    }

    private void ApplyShotEffects(List<TowerEffect> effects, GameObject enemyObj, Vector3 impactPos)
    {
        if (effects == null || effects.Count == 0) return;

        var status = enemyObj.GetComponent<EnemyStatusEffects>();
        if (status == null) status = enemyObj.AddComponent<EnemyStatusEffects>();

        foreach (var e in effects)
        {
            switch (e.effectType)
            {
                case EffectType.Slow: status.ApplySlow(e.effectStrength, e.effectDuration); break;
                case EffectType.DOT: status.ApplyDOT(e.dotDamage, e.effectDuration); break;
                case EffectType.Stun: status.ApplyStun(e.effectDuration); break;
                case EffectType.AOE_Slow: ApplyAOESlow(impactPos, e); break;
                case EffectType.DOT_AOE: ApplyDOTAOE(impactPos, e); break;
                case EffectType.AOE_Stun: ApplyAOEStun(impactPos, e); break;
                case EffectType.AOE_Impact: ApplyAOEImpact(impactPos, e, Mathf.CeilToInt(shot.damage * 0.5f)); break;
                case EffectType.AOE_Front: ApplyAOEImpact(impactPos + transform.right * e.effectRadius, e, Mathf.CeilToInt(shot.damage * 0.5f)); break;
            }
            if (e.effectPrefab != null)
            {
                var vfx = Instantiate(e.effectPrefab, impactPos, Quaternion.identity);
                Destroy(vfx, 2f);
            }
        }
    }

    private void ApplyAOEImpact(Vector3 pos, TowerEffect e, int aoeDamage)
    {
        var colliders = Physics2D.OverlapCircleAll(pos, e.effectRadius);
        foreach (var c in colliders)
        {
            if (!c.CompareTag("Enemy")) continue;
            var enemy = c.GetComponent<Enemy>();
            if (enemy != null) enemy.TakeDamage(aoeDamage);
        }
    }

    private void ApplyAOESlow(Vector3 pos, TowerEffect e)
    {
        var cols = Physics2D.OverlapCircleAll(pos, e.effectRadius);
        foreach (var c in cols)
        {
            if (!c.CompareTag("Enemy")) continue;
            var s = c.GetComponent<EnemyStatusEffects>() ?? c.gameObject.AddComponent<EnemyStatusEffects>();
            s.ApplySlow(e.effectStrength, e.effectDuration);
        }
    }

    private void ApplyDOTAOE(Vector3 pos, TowerEffect e)
    {
        var cols = Physics2D.OverlapCircleAll(pos, e.effectRadius);
        foreach (var c in cols)
        {
            if (!c.CompareTag("Enemy")) continue;
            var s = c.GetComponent<EnemyStatusEffects>() ?? c.gameObject.AddComponent<EnemyStatusEffects>();
            s.ApplyDOT(e.dotDamage, e.effectDuration);
        }
    }

    private void ApplyAOEStun(Vector3 pos, TowerEffect e)
    {
        var cols = Physics2D.OverlapCircleAll(pos, e.effectRadius);
        foreach (var c in cols)
        {
            if (!c.CompareTag("Enemy")) continue;
            var s = c.GetComponent<EnemyStatusEffects>() ?? c.gameObject.AddComponent<EnemyStatusEffects>();
            s.ApplyStun(e.effectDuration);
        }
    }
}