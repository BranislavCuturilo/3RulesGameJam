using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EffectType
{
    None,
    AOE_Front,           
    AOE_Impact,          
    Slow,                
    AOE_Slow,            
    DOT,                 
    DOT_AOE,             
    Stun,                
    AOE_Stun             
}

[System.Serializable]
public class TowerEffect
{
    [Header("Effect Settings")]
    public EffectType effectType = EffectType.None;
    public float effectRadius = 2f;      
    public float effectDuration = 3f;    
    public float effectStrength = 0.5f;  
    public int dotDamage = 5;            
    
    [Header("Visual Effects")]
    public GameObject effectPrefab;      
    public Color effectColor = Color.red;

    [Header("Damage Falloff (AOE_Impact)")]
    [Tooltip("Enable to reduce AOE_Impact damage with distance from impact center.")]
    public bool useDamageFalloff = false;
    [Tooltip("Minimum damage factor at the edge of the radius (0..1). 0 = no damage at edge.")]
    [Range(0f, 1f)] public float minDamageFactor = 0f;
    [Tooltip("Curve exponent for falloff. 1 = linear, 2 = quadratic (stronger falloff), 0.5 = softer.")]
    public float falloffExponent = 1f;
}

public class TowerEffects : MonoBehaviour
{
    [Header("Tower Effects")]
    public TowerEffect[] effects;        
    
    [Header("Projectile Settings")]
    public bool useProjectile = false;
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public bool projectileExplodesOnImpact = false;
    
    private Tower tower;
    
    void Awake()
    {
        tower = GetComponent<Tower>();
    }
    
    public void ApplyAOEFront(TowerEffect effect)
    {
        Vector3 frontPosition = transform.position + transform.right * effect.effectRadius;
        
        Collider2D[] enemies = Physics2D.OverlapCircleAll(frontPosition, effect.effectRadius);
        foreach (var collider in enemies)
        {
            if (collider.CompareTag("Enemy"))
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(tower.Damage);
                }
            }
        }
        
        if (effect.effectPrefab != null)
        {
            GameObject vfx = Instantiate(effect.effectPrefab, frontPosition, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }
    
    public void ApplyAOEImpact(TowerEffect effect, Vector3 impactPosition)
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(impactPosition, effect.effectRadius);
        foreach (var collider in enemies)
        {
            if (collider.CompareTag("Enemy"))
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    float baseAoe = Mathf.Max(0, tower.Damage * 0.5f);
                    float damage = baseAoe;
                    if (effect.useDamageFalloff)
                    {
                        float dist = Vector2.Distance(collider.transform.position, impactPosition);
                        float r = Mathf.Max(0.0001f, effect.effectRadius);
                        float t = Mathf.Clamp01(1f - (dist / r)); 
                        float factor = Mathf.Pow(t, Mathf.Max(0.0001f, effect.falloffExponent));
                        factor = Mathf.Clamp(factor, effect.minDamageFactor, 1f);
                        damage *= factor;
                    }
                    enemy.TakeDamage(Mathf.CeilToInt(damage));
                }
            }
        }
        
        if (effect.effectPrefab != null)
        {
            GameObject vfx = Instantiate(effect.effectPrefab, impactPosition, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }
    
    public void ApplySlow(TowerEffect effect, GameObject target)
    {
        EnemyStatusEffects statusEffects = target.GetComponent<EnemyStatusEffects>();
        if (statusEffects == null)
        {
            statusEffects = target.AddComponent<EnemyStatusEffects>();
        }
        
        statusEffects.ApplySlow(effect.effectStrength, effect.effectDuration);
    }
    
    public void ApplyAOESlow(TowerEffect effect, Vector3 impactPosition)
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(impactPosition, effect.effectRadius);
        foreach (var collider in enemies)
        {
            if (collider.CompareTag("Enemy"))
            {
                ApplySlow(effect, collider.gameObject);
            }
        }
        
        if (effect.effectPrefab != null)
        {
            GameObject vfx = Instantiate(effect.effectPrefab, impactPosition, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }
    
    public void ApplyDOT(TowerEffect effect, GameObject target)
    {
        EnemyStatusEffects statusEffects = target.GetComponent<EnemyStatusEffects>();
        if (statusEffects == null)
        {
            statusEffects = target.AddComponent<EnemyStatusEffects>();
        }
        
        statusEffects.ApplyDOT(effect.dotDamage, effect.effectDuration);
    }
    
    public void ApplyDOTAOE(TowerEffect effect, Vector3 impactPosition)
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(impactPosition, effect.effectRadius);
        foreach (var collider in enemies)
        {
            if (collider.CompareTag("Enemy"))
            {
                ApplyDOT(effect, collider.gameObject);
            }
        }
        
        if (effect.effectPrefab != null)
        {
            GameObject vfx = Instantiate(effect.effectPrefab, impactPosition, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }
    
    public void ApplyStun(TowerEffect effect, GameObject target)
    {
        EnemyStatusEffects statusEffects = target.GetComponent<EnemyStatusEffects>();
        if (statusEffects == null)
        {
            statusEffects = target.AddComponent<EnemyStatusEffects>();
        }
        
        statusEffects.ApplyStun(effect.effectDuration);
    }
    
    public void ApplyAOEStun(TowerEffect effect, Vector3 impactPosition)
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(impactPosition, effect.effectRadius);
        foreach (var collider in enemies)
        {
            if (collider.CompareTag("Enemy"))
            {
                ApplyStun(effect, collider.gameObject);
            }
        }
        
        if (effect.effectPrefab != null)
        {
            GameObject vfx = Instantiate(effect.effectPrefab, impactPosition, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }
    
}

public struct ShotData
{
    public GameObject projectilePrefab;
    public float projectileSpeed;
    public int damage;
    public List<TowerEffect> effects;
}