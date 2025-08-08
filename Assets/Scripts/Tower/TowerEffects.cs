using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EffectType
{
    None,
    AOE_Front,           // AOE ispred tornja
    AOE_Impact,          // AOE oko pogođenog protivnika
    Slow,                // Usporava protivnika
    AOE_Slow,            // AOE usporavanje
    DOT,                 // Damage over time
    DOT_AOE,             // AOE damage over time
    Stun,                // Onesposobljava protivnika
    AOE_Stun             // AOE onesposobljavanje
}

[System.Serializable]
public class TowerEffect
{
    [Header("Effect Settings")]
    public EffectType effectType = EffectType.None;
    public float effectRadius = 2f;      // Radius za AOE efekte
    public float effectDuration = 3f;    // Trajanje efekta (slow, stun, DOT)
    public float effectStrength = 0.5f;  // Jačina efekta (slow factor, DOT damage per second)
    public int dotDamage = 5;            // DOT damage po sekundi
    
    [Header("Visual Effects")]
    public GameObject effectPrefab;      // Prefab za vizuelni efekat
    public Color effectColor = Color.red;
}

public class TowerEffects : MonoBehaviour
{
    [Header("Tower Effects")]
    public TowerEffect[] effects;        // Niz efekata koje toranj može da koristi
    
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
    
    // Više ne primjenjuje efekte direktno iz tornja; efekte primjenjuje Projectile korištenjem ShotData kopija
    
    // Pomoćne metode ostaju za referencu i korištenje iz Projectile-a ukoliko želiš re-use
    
    public void ApplyAOEFront(TowerEffect effect)
    {
        // AOE ispred tornja u pravcu gledanja
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
        
        // Vizuelni efekat
        if (effect.effectPrefab != null)
        {
            GameObject vfx = Instantiate(effect.effectPrefab, frontPosition, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }
    
    public void ApplyAOEImpact(TowerEffect effect, Vector3 impactPosition)
    {
        // AOE oko pogođenog protivnika
        Collider2D[] enemies = Physics2D.OverlapCircleAll(impactPosition, effect.effectRadius);
        foreach (var collider in enemies)
        {
            if (collider.CompareTag("Enemy"))
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    // Smanji damage za AOE (50% od osnovnog)
                    int aoeDamage = Mathf.RoundToInt(tower.Damage * 0.5f);
                    enemy.TakeDamage(aoeDamage);
                }
            }
        }
        
        // Vizuelni efekat
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
        
        // Vizuelni efekat
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
        
        // Vizuelni efekat
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
        
        // Vizuelni efekat
        if (effect.effectPrefab != null)
        {
            GameObject vfx = Instantiate(effect.effectPrefab, impactPosition, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }
    
    // OnFire uklonjen – logika pucanja premještena u Tower i Projectile
}

public struct ShotData
{
    public GameObject projectilePrefab;
    public float projectileSpeed;
    public int damage;
    public List<TowerEffect> effects;
}