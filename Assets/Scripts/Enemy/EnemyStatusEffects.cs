using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class StatusEffect
{
    public string effectName;
    public float duration;
    public float remainingTime;
    public bool isActive;
    
    public StatusEffect(string name, float dur)
    {
        effectName = name;
        duration = dur;
        remainingTime = dur;
        isActive = true;
    }
}

public class EnemyStatusEffects : MonoBehaviour
{
    [Header("Status Effects")]
    public bool isSlowed = false;
    public bool isStunned = false;
    public bool hasDOT = false;
    
    [Header("Effect Values")]
    public float slowFactor = 1f;        // 1f = normalna brzina, 0.5f = 50% sporije
    public float originalSpeed;
    public int dotDamagePerSecond = 0;
    
    [Header("Visual Indicators")]
    public GameObject slowEffect;
    public GameObject stunEffect;
    public GameObject dotEffect;
    
    private Enemy enemy;
    private List<StatusEffect> activeEffects = new List<StatusEffect>();
    private Coroutine dotCoroutine;
    
    void Awake()
    {
        enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            originalSpeed = enemy.movespeed;
        }
    }
    
    void Update()
    {
        UpdateEffects();
    }
    
    private void UpdateEffects()
    {
        // Ažuriraj sve efekte
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].remainingTime -= Time.deltaTime;
            
            if (activeEffects[i].remainingTime <= 0)
            {
                RemoveEffect(activeEffects[i]);
                activeEffects.RemoveAt(i);
            }
        }
        
        // Primijeni current effects
        ApplyCurrentEffects();
    }
    
    public void ApplySlow(float slowStrength, float duration)
    {
        // Ukloni postojeći slow ako postoji
        RemoveEffectByName("Slow");
        
        // Dodaj novi slow
        StatusEffect slowEffect = new StatusEffect("Slow", duration);
        activeEffects.Add(slowEffect);
        
        slowFactor = slowStrength; // 0.5f = 50% brzine
        isSlowed = true;
        
        // Vizuelni efekat
        if (this.slowEffect != null)
        {
            this.slowEffect.SetActive(true);
        }
        
        Debug.Log($"Enemy slowed: {slowStrength * 100}% speed for {duration}s");
    }
    
    public void ApplyStun(float duration)
    {
        // Ukloni postojeći stun ako postoji
        RemoveEffectByName("Stun");
        
        // Dodaj novi stun
        StatusEffect stunEffect = new StatusEffect("Stun", duration);
        activeEffects.Add(stunEffect);
        
        isStunned = true;
        
        // Vizuelni efekat
        if (this.stunEffect != null)
        {
            this.stunEffect.SetActive(true);
        }
        
        Debug.Log($"Enemy stunned for {duration}s");
    }
    
    public void ApplyDOT(int damagePerSecond, float duration)
    {
        // Ukloni postojeći DOT ako postoji
        RemoveEffectByName("DOT");
        if (dotCoroutine != null)
        {
            StopCoroutine(dotCoroutine);
        }
        
        // Dodaj novi DOT
        StatusEffect dotEffect = new StatusEffect("DOT", duration);
        activeEffects.Add(dotEffect);
        
        dotDamagePerSecond = damagePerSecond;
        hasDOT = true;
        
        // Pokreni DOT coroutine
        dotCoroutine = StartCoroutine(DOTCoroutine(damagePerSecond, duration));
        
        // Vizuelni efekat
        if (this.dotEffect != null)
        {
            this.dotEffect.SetActive(true);
        }
        
        Debug.Log($"Enemy DOT: {damagePerSecond} damage/s for {duration}s");
    }
    
    private IEnumerator DOTCoroutine(int damage, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration && enemy != null)
        {
            yield return new WaitForSeconds(1f); // Svaku sekundu
            elapsed += 1f;
            
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                
                // Vizuelni efekat za damage
                CreateDamageEffect();
            }
        }
        
        // Ukloni DOT nakon završetka
        RemoveEffectByName("DOT");
    }
    
    private void CreateDamageEffect()
    {
        // Kreirati floating damage text ili particle efekat
        // Za sada samo debug
        Debug.Log($"DOT Damage: {dotDamagePerSecond}");
    }
    
    private void ApplyCurrentEffects()
    {
        if (enemy == null) return;
        
        // Primijeni slow efekat
        if (isSlowed)
        {
            enemy.movespeed = originalSpeed * slowFactor;
        }
        else
        {
            enemy.movespeed = originalSpeed;
        }
        
        // Primijeni stun efekat (zaustavi protivnika)
        if (isStunned)
        {
            enemy.movespeed = 0f;
        }
    }
    
    private void RemoveEffect(StatusEffect effect)
    {
        switch (effect.effectName)
        {
            case "Slow":
                isSlowed = false;
                slowFactor = 1f;
                if (slowEffect != null) slowEffect.SetActive(false);
                break;
                
            case "Stun":
                isStunned = false;
                if (stunEffect != null) stunEffect.SetActive(false);
                break;
                
            case "DOT":
                hasDOT = false;
                dotDamagePerSecond = 0;
                if (dotEffect != null) dotEffect.SetActive(false);
                if (dotCoroutine != null)
                {
                    StopCoroutine(dotCoroutine);
                    dotCoroutine = null;
                }
                break;
        }
    }
    
    private void RemoveEffectByName(string effectName)
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (activeEffects[i].effectName == effectName)
            {
                RemoveEffect(activeEffects[i]);
                activeEffects.RemoveAt(i);
                break;
            }
        }
    }
    
    // Getter metode za eksterni pristup
    public bool IsSlowed() => isSlowed;
    public bool IsStunned() => isStunned;
    public bool HasDOT() => hasDOT;
    public float GetSlowFactor() => slowFactor;
    
    void OnDestroy()
    {
        if (dotCoroutine != null)
        {
            StopCoroutine(dotCoroutine);
        }
    }
}