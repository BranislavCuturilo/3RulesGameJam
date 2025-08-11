using UnityEngine;

[System.Serializable]
public class ProgressionLevel
{
    [Header("Level Information")]
    [TextArea(2, 4)]
    public string levelDescription = "Opisuje kako se mijenjaju pravila na ovom nivou";
    
    [Header("Enemy segment (globalno na sve protivnike)")]
    [Range(0.2f, 5f)]
    public float enemySpeedMultiplier = 1f;
    [Range(0.2f, 5f)]
    public float enemyHealthMultiplier = 1f;
    [Range(0.2f, 5f)]
    public float enemyQuantityMultiplier = 1f;
    [Range(0.2f, 5f)]
    public float enemyMoneyValueMultiplier = 1f;
    [Header("Promena leak dmg (×0.2 do ×5) – rezultat se zaokružuje na ≥ 1")]
    [Range(0.2f, 5f)]
    public float enemyLeakDamageMultiplier = 1f;
    
    [Header("Enemy Count Override (optional)")]
    [Tooltip("If enabled, this level will force a fixed total number of enemies in the wave.")]
    public bool useFixedEnemyCount = false;
    [Tooltip("Total number of enemies to spawn this wave if override is enabled.")]
    public int fixedEnemyCount = 0;

    [Header("Enemy HP Override (optional)")]
    [Tooltip("If enabled, every enemy this wave will use a fixed HP value (overrides multipliers).")]
    public bool useFixedEnemyHealth = false;
    [Tooltip("Fixed HP for all enemies in the wave when override is enabled.")]
    public int fixedEnemyHealth = 0;

    [Header("Enemy Leak Damage Override (optional)")]
    [Tooltip("If enabled, all enemies this level deal a fixed amount of damage to the player when they leak, ignoring multipliers.")]
    public bool useFixedLeakDamage = false;
    [Tooltip("Fixed damage each enemy does to player on leak for this level if override is enabled.")]
    public int fixedLeakDamage = 1;

    [Header("Enemy Spawn Timing (optional)")]
    [Tooltip("If enabled, overrides enemy spawn wait time with a fixed random interval per enemy in this wave.")]
    public bool useFixedSpawnDelay = false;
    [Range(0.01f, 5f)] public float fixedSpawnDelayMin = 0.75f;
    [Range(0.01f, 5f)] public float fixedSpawnDelayMax = 1.0f;
    
    [Header("Tower segment (globalno na sve towere)")]
    [Range(0.2f, 5f)]
    public float towerFireRateMultiplier = 1f;
    [Range(0.2f, 5f)]
    public float towerDamageMultiplier = 1f;
    [Range(0.2f, 5f)]
    public float towerRangeMultiplier = 1f;
    [Range(0.2f, 5f)]
    public float towerPlacementCostMultiplier = 1f;
    [Tooltip("Global force targeting for all towers this level (NoChange = ne menja)")]
    public TargetingMode forcedTargetingMode = TargetingMode.NoChange;
    
    [Header("Promena efekata (za sve towere sa efektima)")]
    [Range(0.2f, 5f)]
    public float dotDamageMultiplier = 1f;
    [Range(0.2f, 5f)]
    public float slowEffectMultiplier = 1f;
    [Range(0.2f, 5f)]
    public float stunDurationMultiplier = 1f;
    [Range(0.2f, 5f)]
    public float aoeRadiusMultiplier = 1f;
    [Range(0.2f, 5f)]
    public float projectileSpeedMultiplier = 1f;
    [Tooltip("Global effect duration multiplier for all effects (Slow, DOT, Stun, AOE variants).")]
    [Range(0.2f, 5f)]
    public float effectDurationMultiplier = 1f;
    
    [Header("Economy segment (globalne izmene)")]
    [Range(0.2f, 5f)]
    public float waveCompleteMoneyMultiplier = 1f;
    [Range(0.2f, 5f)]
    public float enemyKillMoneyMultiplier = 1f;
    [Range(0.2f, 5f)]
    public float upgradeDiscountMultiplier = 1f;

    [Header("Economy Fixed Values (optional)")]
    [Tooltip("If enabled, gives a fixed amount of money at the end of the wave, ignoring multipliers.")]
    public bool useFixedWaveCompleteMoney = false;
    public int fixedWaveCompleteMoney = 0;
    [Tooltip("If enabled, each enemy gives a fixed kill reward this wave, ignoring multipliers.")]
    public bool useFixedEnemyKillMoney = false;
    public int fixedEnemyKillMoney = 0;
}

public enum TowerType
{
    All,
    CannonTower,
    SniperTower,
    MachineGunTower,
    ShotgunTower
}

public enum EnemyType
{
    All,
    Enemy,
    FastEnemy,
    TankEnemy
}

public enum TargetingMode
{
    NoChange,
    First,
    Last,
    Strongest
}

[System.Serializable]
public class EnemyRule
{
    [Header("Enemy Rule Description")]
    public string ruleName = "Enemy Rule";
    [TextArea(1, 3)]
    public string ruleDescription = "Modifikuje protivnike";
    
    [Header("Target")]
    public EnemyType targetEnemyType = EnemyType.All;
    
    [Header("Enemy Modifiers")]
    public float speedMultiplier = 1f;
    public float healthMultiplier = 1f;
    public float quantityMultiplier = 1f; 
    public float moneyValueMultiplier = 1f; 
    public float leakDamageMultiplier = 1f; 

    [Header("Leak Damage Override")]
    public bool useFixedLeakDamage = false; 
    public int fixedLeakDamage = 0;         

    [Header("Spawn Delay Override")]
    public bool useFixedSpawnDelay = false;
    public float fixedSpawnDelayMin = 0.75f;
    public float fixedSpawnDelayMax = 1.0f;

    [Header("Enemy Count Override")]
    public bool useFixedEnemyCount = false; 
    public int fixedEnemyCount = 0;         

    [Header("Enemy HP Override")]
    public bool useFixedEnemyHealth = false; 
    public int fixedEnemyHealth = 0;         
}

[System.Serializable]
public class TowerRule
{
    [Header("Tower Rule Description")]
    public string ruleName = "Tower Rule";
    [TextArea(1, 3)]
    public string ruleDescription = "Modifikuje tornjeve";
    
    [Header("Target")]
    public TowerType targetTowerType = TowerType.All;
    
    [Header("Tower Modifiers")]
    public float fireRateMultiplier = 1f;
    public float damageMultiplier = 1f;
    public float rangeMultiplier = 1f;
    public float placementCostMultiplier = 1f;
    public TargetingMode forceTargetingMode = TargetingMode.NoChange;
    
    [Header("Advanced Effects")]
    public float dotDamageMultiplier = 1f; 
    public float slowEffectMultiplier = 1f; 
    public float stunDurationMultiplier = 1f; 
    public float aoeRadiusMultiplier = 1f; 
    public float projectileSpeedMultiplier = 1f; 
    public float effectDurationMultiplier = 1f; 
}

[System.Serializable]
public class EconomyRule
{
    [Header("Economy Rule Description")]
    public string ruleName = "Economy Rule";
    [TextArea(1, 3)]
    public string ruleDescription = "Modifikuje ekonomiju";
    
    [Header("Economy Modifiers")]
    public float waveCompleteMoneyMultiplier = 1f; 
    public float enemyKillMoneyMultiplier = 1f; 
    public float towerPlacementCostMultiplier = 1f; 
    public float upgradeDiscountMultiplier = 1f; 

    [Header("Fixed Economy Overrides (optional)")]
    public bool useFixedWaveCompleteMoney = false;
    public int fixedWaveCompleteMoney = 0;
    public bool useFixedEnemyKillMoney = false;
    public int fixedEnemyKillMoney = 0;
}

[CreateAssetMenu(fileName = "New Rule Set", menuName = "Tower Defense/Rule Set")]
public class Rule : ScriptableObject
{
    [Header("Rule Set Information")]
    public string baseRuleSetName = "New Rule Set"; 
    public Sprite ruleSetImage; 
    
    [Header("Manual Progression Levels")]
    public ProgressionLevel[] progressionLevels = new ProgressionLevel[10]; 
    
    public string GetDisplayText(int progressionLevel = 0)
    {
        if (progressionLevel <= 0)
        {
            return "Osnovni nivo - bez progresije";
        }
        
        if (progressionLevels == null || progressionLevel > progressionLevels.Length || progressionLevels[progressionLevel - 1] == null)
        {
            return "Nivo nije definisan";
        }
        
        return progressionLevels[progressionLevel - 1].levelDescription;
    }
    
    public string GetRuleSetName(int progressionLevel = 0)
    {
        string romanNumeral = GetRomanNumeral(progressionLevel + 1);
        return $"{baseRuleSetName} {romanNumeral}";
    }
    
    private string GetRomanNumeral(int number)
    {
        switch (number)
        {
            case 1: return "I";
            case 2: return "II";
            case 3: return "III";
            case 4: return "IV";
            case 5: return "V";
            case 6: return "VI";
            case 7: return "VII";
            case 8: return "VIII";
            case 9: return "IX";
            case 10: return "X";
            default: return number.ToString();
        }
    }
    
    public void ApplyRules(int progressionLevel = 0)
    {
        if (RuleManager.main != null)
        {
            RuleManager.main.ApplyRuleSet(this, progressionLevel);
        }
    }
    
    public EnemyRule GetProgressedEnemyRule(int progressionLevel)
    {
        if (progressionLevel <= 0)
        {
            return new EnemyRule(); 
        }
        
        if (progressionLevels == null || progressionLevel > progressionLevels.Length || progressionLevels[progressionLevel - 1] == null)
        {
            Debug.LogWarning($"Progression level {progressionLevel} nije definisan za {baseRuleSetName}. Koristim default vrijednosti.");
            return new EnemyRule();
        }
        
        ProgressionLevel level = progressionLevels[progressionLevel - 1];
        
        EnemyRule progressed = new EnemyRule();
        progressed.ruleName = $"Enemy Rule Level {progressionLevel}";
        progressed.ruleDescription = $"Enemy modifikatori za nivo {progressionLevel}";
        progressed.targetEnemyType = EnemyType.All;
        
        progressed.speedMultiplier = level.enemySpeedMultiplier;
        progressed.healthMultiplier = level.enemyHealthMultiplier;
        progressed.quantityMultiplier = level.enemyQuantityMultiplier;
        progressed.moneyValueMultiplier = level.enemyMoneyValueMultiplier;
        progressed.leakDamageMultiplier = level.enemyLeakDamageMultiplier;
        
        progressed.useFixedLeakDamage = level.useFixedLeakDamage;
        progressed.fixedLeakDamage = level.fixedLeakDamage;

        progressed.useFixedEnemyCount = level.useFixedEnemyCount;
        progressed.fixedEnemyCount = level.fixedEnemyCount;

        progressed.useFixedEnemyHealth = level.useFixedEnemyHealth;
        progressed.fixedEnemyHealth = level.fixedEnemyHealth;

        progressed.useFixedSpawnDelay = level.useFixedSpawnDelay;
        progressed.fixedSpawnDelayMin = level.fixedSpawnDelayMin;
        progressed.fixedSpawnDelayMax = level.fixedSpawnDelayMax;
        
        return progressed;
    }
    
    public TowerRule GetProgressedTowerRule(int progressionLevel)
    {
        if (progressionLevel <= 0)
        {
            return new TowerRule(); 
        }
        
        if (progressionLevels == null || progressionLevel > progressionLevels.Length || progressionLevels[progressionLevel - 1] == null)
        {
            Debug.LogWarning($"Progression level {progressionLevel} nije definisan za {baseRuleSetName}. Koristim default vrijednosti.");
            return new TowerRule();
        }
        
        ProgressionLevel level = progressionLevels[progressionLevel - 1];
        
        TowerRule progressed = new TowerRule();
        progressed.ruleName = $"Tower Rule Level {progressionLevel}";
        progressed.ruleDescription = $"Tower modifikatori za nivo {progressionLevel}";
        progressed.targetTowerType = TowerType.All;
        progressed.forceTargetingMode = level.forcedTargetingMode;
        
        progressed.fireRateMultiplier = level.towerFireRateMultiplier;
        progressed.damageMultiplier = level.towerDamageMultiplier;
        progressed.rangeMultiplier = level.towerRangeMultiplier;
        progressed.placementCostMultiplier = level.towerPlacementCostMultiplier;
        
        progressed.dotDamageMultiplier = level.dotDamageMultiplier;
        progressed.slowEffectMultiplier = level.slowEffectMultiplier;
        progressed.stunDurationMultiplier = level.stunDurationMultiplier;
        progressed.aoeRadiusMultiplier = level.aoeRadiusMultiplier;
        progressed.projectileSpeedMultiplier = level.projectileSpeedMultiplier;
        progressed.effectDurationMultiplier = level.effectDurationMultiplier;
        
        return progressed;
    }
    
    public EconomyRule GetProgressedEconomyRule(int progressionLevel)
    {
        if (progressionLevel <= 0)
        {
            return new EconomyRule(); 
        }
        
        if (progressionLevels == null || progressionLevel > progressionLevels.Length || progressionLevels[progressionLevel - 1] == null)
        {
            Debug.LogWarning($"Progression level {progressionLevel} nije definisan za {baseRuleSetName}. Koristim default vrijednosti.");
            return new EconomyRule();
        }
        
        ProgressionLevel level = progressionLevels[progressionLevel - 1];
        
        EconomyRule progressed = new EconomyRule();
        progressed.ruleName = $"Economy Rule Level {progressionLevel}";
        progressed.ruleDescription = $"Economy modifikatori za nivo {progressionLevel}";
        
        progressed.waveCompleteMoneyMultiplier = level.waveCompleteMoneyMultiplier;
        progressed.enemyKillMoneyMultiplier = level.enemyKillMoneyMultiplier;
        progressed.upgradeDiscountMultiplier = level.upgradeDiscountMultiplier;
        progressed.towerPlacementCostMultiplier = level.towerPlacementCostMultiplier;
        progressed.useFixedWaveCompleteMoney = level.useFixedWaveCompleteMoney;
        progressed.fixedWaveCompleteMoney = level.fixedWaveCompleteMoney;
        progressed.useFixedEnemyKillMoney = level.useFixedEnemyKillMoney;
        progressed.fixedEnemyKillMoney = level.fixedEnemyKillMoney;
        
        return progressed;
    }
    
    public void CreateDefaultProgressionLevels()
    {
        progressionLevels = new ProgressionLevel[10];
        
        for (int i = 0; i < progressionLevels.Length; i++)
        {
            progressionLevels[i] = new ProgressionLevel();
            progressionLevels[i].levelDescription = $"Opis modifikatora za nivo {i + 1}";
            
        }
    }
    
    public int GetMaxProgressionLevel()
    {
        return progressionLevels?.Length ?? 0;
    }
}