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
    
    public void ApplyEffects(GameObject target, Vector3 impactPosition)
    {
        if (effects == null || effects.Length == 0) return;
        
        foreach (var effect in effects)
        {
            ApplyEffect(effect, target, impactPosition);
        }
    }
    
    private void ApplyEffect(TowerEffect effect, GameObject target, Vector3 impactPosition)
    {
        switch (effect.effectType)
        {
            case EffectType.AOE_Front:
                ApplyAOEFront(effect);
                break;
                
            case EffectType.AOE_Impact:
                ApplyAOEImpact(effect, impactPosition);
                break;
                
            case EffectType.Slow:
                ApplySlow(effect, target);
                break;
                
            case EffectType.AOE_Slow:
                ApplyAOESlow(effect, impactPosition);
                break;
                
            case EffectType.DOT:
                ApplyDOT(effect, target);
                break;
                
            case EffectType.DOT_AOE:
                ApplyDOTAOE(effect, impactPosition);
                break;
                
            case EffectType.Stun:
                ApplyStun(effect, target);
                break;
                
            case EffectType.AOE_Stun:
                ApplyAOEStun(effect, impactPosition);
                break;
        }
    }
    
    private void ApplyAOEFront(TowerEffect effect)
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
    
    private void ApplyAOEImpact(TowerEffect effect, Vector3 impactPosition)
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
    
    private void ApplySlow(TowerEffect effect, GameObject target)
    {
        EnemyStatusEffects statusEffects = target.GetComponent<EnemyStatusEffects>();
        if (statusEffects == null)
        {
            statusEffects = target.AddComponent<EnemyStatusEffects>();
        }
        
        statusEffects.ApplySlow(effect.effectStrength, effect.effectDuration);
    }
    
    private void ApplyAOESlow(TowerEffect effect, Vector3 impactPosition)
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
    
    private void ApplyDOT(TowerEffect effect, GameObject target)
    {
        EnemyStatusEffects statusEffects = target.GetComponent<EnemyStatusEffects>();
        if (statusEffects == null)
        {
            statusEffects = target.AddComponent<EnemyStatusEffects>();
        }
        
        statusEffects.ApplyDOT(effect.dotDamage, effect.effectDuration);
    }
    
    private void ApplyDOTAOE(TowerEffect effect, Vector3 impactPosition)
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
    
    private void ApplyStun(TowerEffect effect, GameObject target)
    {
        EnemyStatusEffects statusEffects = target.GetComponent<EnemyStatusEffects>();
        if (statusEffects == null)
        {
            statusEffects = target.AddComponent<EnemyStatusEffects>();
        }
        
        statusEffects.ApplyStun(effect.effectDuration);
    }
    
    private void ApplyAOEStun(TowerEffect effect, Vector3 impactPosition)
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
    
    // Pozovi ovo kada toranj puca (iz Tower.cs)
    public void OnFire(GameObject target)
    {
        if (useProjectile && projectilePrefab != null)
        {
            // Ispali projektil
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript == null)
            {
                projScript = projectile.AddComponent<Projectile>();
            }
            
            projScript.Initialize(target, projectileSpeed, tower.Damage, this);
        }
        else
        {
            // Trenutni hit bez projektila
            ApplyEffects(target, target.transform.position);
        }
    }
}