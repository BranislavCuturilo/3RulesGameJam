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
    public float slowFactor = 1f;        
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
    }

    void Start()
    {
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
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].remainingTime -= Time.deltaTime;
            
            if (activeEffects[i].remainingTime <= 0)
            {
                RemoveEffect(activeEffects[i]);
                activeEffects.RemoveAt(i);
            }
        }
        
        ApplyCurrentEffects();
    }
    
    public void ApplySlow(float slowStrength, float duration)
    {
        RemoveEffectByName("Slow");
        
        StatusEffect slowEffect = new StatusEffect("Slow", duration);
        activeEffects.Add(slowEffect);
        
        slowFactor = slowStrength; 
        isSlowed = true;
        
        if (this.slowEffect != null)
        {
            this.slowEffect.SetActive(true);
        }
        
        Debug.Log($"Enemy slowed: {slowStrength * 100}% speed for {duration}s");
    }
    
    public void ApplyStun(float duration)
    {
        RemoveEffectByName("Stun");
        
        StatusEffect stunEffect = new StatusEffect("Stun", duration);
        activeEffects.Add(stunEffect);
        
        isStunned = true;
        
        if (this.stunEffect != null)
        {
            this.stunEffect.SetActive(true);
        }
        
        Debug.Log($"Enemy stunned for {duration}s");
    }
    
    public void ApplyDOT(int damagePerSecond, float duration)
    {
        RemoveEffectByName("DOT");
        if (dotCoroutine != null)
        {
            StopCoroutine(dotCoroutine);
        }
        
        StatusEffect dotEffect = new StatusEffect("DOT", duration);
        activeEffects.Add(dotEffect);
        
        dotDamagePerSecond = damagePerSecond;
        hasDOT = true;
        
        dotCoroutine = StartCoroutine(DOTCoroutine(damagePerSecond, duration));
        
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
            yield return new WaitForSeconds(1f); 
            elapsed += 1f;
            
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                
                CreateDamageEffect();
            }
        }
        
        RemoveEffectByName("DOT");
    }
    
    private void CreateDamageEffect()
    {
        Debug.Log($"DOT Damage: {dotDamagePerSecond}");
    }
    
    private void ApplyCurrentEffects()
    {
        if (enemy == null) return;
        
        float newSpeed = originalSpeed;
        
        if (isSlowed)
        {
            newSpeed = originalSpeed * slowFactor;
        }
        
        if (isStunned)
        {
            newSpeed = 0f;
        }
        
        enemy.movespeed = newSpeed;
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

    public void SetBaseSpeed(float speed)
    {
        originalSpeed = speed;
        
        ApplyCurrentEffects();
    }
}