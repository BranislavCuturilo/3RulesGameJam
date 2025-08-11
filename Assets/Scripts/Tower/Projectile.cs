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

    [Header("Rotation")]
    [Tooltip("If your sprite/collider is oriented UP in the prefab, enable this to rotate using UP axis instead of RIGHT.")]
    [SerializeField] private bool useUpAxisForRotation = false;
    [Tooltip("Additional Z rotation offset in degrees to fine-tune alignment (e.g., 90 if needed).")]
    [SerializeField] private float rotationOffsetDegrees = 0f;

    [Header("Beam / Hitscan")]
    [Tooltip("If enabled, projectile will render as an instant beam from spawn to target, apply hit immediately, and not move.")]
    [SerializeField] private bool isInstantBeam = false;
    [Tooltip("Beam lifetime in seconds (how long the beam sprite stays visible).")]
    [SerializeField] private float beamLifetime = 0.06f;
    [Tooltip("Visual thickness scale on Y for the beam sprite.")]
    [SerializeField] private float beamThickness = 0.12f;

    private const float maxLifeSeconds = 5f;

    public void Initialize(ShotData shotData, GameObject targetEnemy)
    {
        shot = shotData;
        target = targetEnemy;

        if (target != null)
        {
            lastKnownTargetPos = target.transform.position;

            Vector3 dir = (lastKnownTargetPos - transform.position).normalized;
            ApplyRotation(dir);
        }

        if (isInstantBeam)
        {
            RenderBeamAndHit();
        }
        else
        {
            Destroy(gameObject, maxLifeSeconds);
        }
    }

    void Update()
    {
        if (isInstantBeam) return; // no movement for beams

        if (target != null)
        {
            lastKnownTargetPos = target.transform.position;
        }

        Vector3 direction = (lastKnownTargetPos - transform.position).normalized;
        transform.position += direction * shot.projectileSpeed * Time.deltaTime;
        ApplyRotation(direction);
    }

    private void RenderBeamAndHit()
    {
        // Compute from current spawn position to target
        Vector3 startPos = transform.position;
        Vector3 endPos = lastKnownTargetPos;
        Vector3 delta = endPos - startPos;
        float distance = delta.magnitude;

        if (distance > 0.0001f)
        {
            // Place beam at midpoint
            Vector3 mid = startPos + delta * 0.5f;
            transform.position = mid;

            // Rotate to face target
            Vector3 dir = delta.normalized;
            ApplyRotation(dir);

            // Scale along X to cover distance, Y for thickness
            Vector3 scale = transform.localScale;
            scale.x = distance;
            scale.y = beamThickness;
            transform.localScale = scale;
        }

        // Apply hit immediately to the target (if still valid)
        if (target != null)
        {
            var enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(shot.damage);
                ApplyShotEffects(shot.effects, enemy.gameObject, target.transform.position);
            }
        }

        // Optional explosion VFX at target (match beam lifetime so it doesn't linger)
        if (explosionEffect != null)
        {
            var fx = Instantiate(explosionEffect, endPos, Quaternion.identity);
            Destroy(fx, Mathf.Max(0.05f, beamLifetime));
        }

        // Disable trail immediately for beams (prevent afterimage)
        if (trail != null)
        {
            trail.emitting = false;
            trail.time = Mathf.Min(trail.time, beamLifetime);
        }

        // Auto-destroy beam quickly
        Destroy(gameObject, Mathf.Max(0.01f, beamLifetime));
    }

    private void ApplyRotation(Vector3 dir)
    {
        if (dir.sqrMagnitude <= 0f) return;
        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; // angle for RIGHT axis
        if (useUpAxisForRotation)
        {
            angleDeg -= 90f; // rotate so UP points along velocity
        }
        angleDeg += rotationOffsetDegrees;
        transform.rotation = Quaternion.AngleAxis(angleDeg, Vector3.forward);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isInstantBeam) return; // beams don't use triggers
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
            if (enemy != null)
            {
                float damage = aoeDamage;
                if (e.useDamageFalloff)
                {
                    float dist = Vector2.Distance(c.transform.position, pos);
                    float r = Mathf.Max(0.0001f, e.effectRadius);
                    float t = Mathf.Clamp01(1f - (dist / r)); // 1 in center, 0 at edge
                    float factor = Mathf.Pow(t, Mathf.Max(0.0001f, e.falloffExponent));
                    factor = Mathf.Clamp(factor, e.minDamageFactor, 1f);
                    damage *= factor;
                }
                enemy.TakeDamage(Mathf.CeilToInt(damage));
            }
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