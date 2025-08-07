using UnityEngine;

[System.Serializable]
public class ProgressionLevel
{
    [Header("Level Information")]
    [TextArea(2, 4)]
    public string levelDescription = "Opisuje kako se mijenjaju pravila na ovom nivou";
    
    [Header("Enemy Progression Multipliers")]
    [Range(0.1f, 5f)]
    public float enemySpeedMultiplier = 1f;
    [Range(0.1f, 5f)]
    public float enemyHealthMultiplier = 1f;
    [Range(0.1f, 5f)]
    public float enemyDamageMultiplier = 1f;
    [Range(0.1f, 5f)]
    public float enemyQuantityMultiplier = 1f;
    [Range(0.1f, 5f)]
    public float enemyMoneyValueMultiplier = 1f;
    
    [Header("Tower Progression Multipliers")]
    [Range(0.1f, 5f)]
    public float towerFireRateMultiplier = 1f;
    [Range(0.1f, 5f)]
    public float towerDamageMultiplier = 1f;
    [Range(0.1f, 5f)]
    public float towerRangeMultiplier = 1f;
    [Range(0.1f, 5f)]
    public float towerPlacementCostMultiplier = 1f;
    
    [Header("Advanced Tower Effects")]
    [Range(0.1f, 5f)]
    public float dotDamageMultiplier = 1f;
    [Range(0.1f, 5f)]
    public float slowEffectMultiplier = 1f;
    [Range(0.1f, 5f)]
    public float stunDurationMultiplier = 1f;
    [Range(0.1f, 5f)]
    public float aoeRadiusMultiplier = 1f;
    [Range(0.1f, 5f)]
    public float projectileSpeedMultiplier = 1f;
    
    [Header("Economy Progression Multipliers")]
    [Range(0.1f, 5f)]
    public float waveCompleteMoneyMultiplier = 1f;
    [Range(0.1f, 5f)]
    public float enemyKillMoneyMultiplier = 1f;
    [Range(0.1f, 5f)]
    public float upgradeDiscountMultiplier = 1f;
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
    public float damageMultiplier = 1f;
    public float quantityMultiplier = 1f; // Za broj protivnika u valu
    public float moneyValueMultiplier = 1f; // Koliko novca daju kad umru
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
    public float dotDamageMultiplier = 1f; // Damage over time
    public float slowEffectMultiplier = 1f; // Slow effect strength
    public float stunDurationMultiplier = 1f; // Stun duration
    public float aoeRadiusMultiplier = 1f; // AOE radius
    public float projectileSpeedMultiplier = 1f; // Projectile speed
}

[System.Serializable]
public class EconomyRule
{
    [Header("Economy Rule Description")]
    public string ruleName = "Economy Rule";
    [TextArea(1, 3)]
    public string ruleDescription = "Modifikuje ekonomiju";
    
    [Header("Economy Modifiers")]
    public float waveCompleteMoneyMultiplier = 1f; // Bonus na kraju runde
    public float enemyKillMoneyMultiplier = 1f; // Novac od ubijenih protivnika
    public float towerPlacementCostMultiplier = 1f; // Cijena postavljanja tornjeva
    public float upgradeDiscountMultiplier = 1f; // Popust na nadogradnje (0.8f = 20% popust)
}

[CreateAssetMenu(fileName = "New Rule Set", menuName = "Tower Defense/Rule Set")]
public class Rule : ScriptableObject
{
    [Header("Rule Set Information")]
    public string baseRuleSetName = "New Rule Set"; // Default ime bez levela
    public Sprite ruleSetImage; // Ikonica je uvek ista za sve levele
    
    [Header("Manual Progression Levels")]
    public ProgressionLevel[] progressionLevels = new ProgressionLevel[5]; // Manuelni nivoi progresije
    
    // Removed display options - now handled automatically per level
    
    public string GetDisplayText(int progressionLevel = 0)
    {
        // Ako je level 0, vrati osnovnu poruku
        if (progressionLevel <= 0)
        {
            return "Osnovni nivo - bez progresije";
        }
        
        // Provjeri da li postoji level
        if (progressionLevels == null || progressionLevel > progressionLevels.Length || progressionLevels[progressionLevel - 1] == null)
        {
            return "Nivo nije definisan";
        }
        
        // Vrati opis konkretnog levela
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
    
    // Izračunaj progresirane modifikatore za enemy pravila
    public EnemyRule GetProgressedEnemyRule(int progressionLevel)
    {
        // Za level 0, vrati default vrijednosti
        if (progressionLevel <= 0)
        {
            return new EnemyRule(); // Default EnemyRule sa 1f vrijednostima
        }
        
        // Provjeri da li postoji definisan nivo
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
        
        // Direktno koristi multiplikatore iz levela
        progressed.speedMultiplier = level.enemySpeedMultiplier;
        progressed.healthMultiplier = level.enemyHealthMultiplier;
        progressed.damageMultiplier = level.enemyDamageMultiplier;
        progressed.quantityMultiplier = level.enemyQuantityMultiplier;
        progressed.moneyValueMultiplier = level.enemyMoneyValueMultiplier;
        
        return progressed;
    }
    
    // Izračunaj progresirane modifikatore za tower pravila
    public TowerRule GetProgressedTowerRule(int progressionLevel)
    {
        // Za level 0, vrati default vrijednosti
        if (progressionLevel <= 0)
        {
            return new TowerRule(); // Default TowerRule sa 1f vrijednostima
        }
        
        // Provjeri da li postoji definisan nivo
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
        progressed.forceTargetingMode = TargetingMode.NoChange;
        
        // Direktno koristi multiplikatore iz levela
        progressed.fireRateMultiplier = level.towerFireRateMultiplier;
        progressed.damageMultiplier = level.towerDamageMultiplier;
        progressed.rangeMultiplier = level.towerRangeMultiplier;
        progressed.placementCostMultiplier = level.towerPlacementCostMultiplier;
        
        // Advanced effects
        progressed.dotDamageMultiplier = level.dotDamageMultiplier;
        progressed.slowEffectMultiplier = level.slowEffectMultiplier;
        progressed.stunDurationMultiplier = level.stunDurationMultiplier;
        progressed.aoeRadiusMultiplier = level.aoeRadiusMultiplier;
        progressed.projectileSpeedMultiplier = level.projectileSpeedMultiplier;
        
        return progressed;
    }
    
    // Izračunaj progresirane modifikatore za economy pravila
    public EconomyRule GetProgressedEconomyRule(int progressionLevel)
    {
        // Za level 0, vrati default vrijednosti
        if (progressionLevel <= 0)
        {
            return new EconomyRule(); // Default EconomyRule sa 1f vrijednostima
        }
        
        // Provjeri da li postoji definisan nivo
        if (progressionLevels == null || progressionLevel > progressionLevels.Length || progressionLevels[progressionLevel - 1] == null)
        {
            Debug.LogWarning($"Progression level {progressionLevel} nije definisan za {baseRuleSetName}. Koristim default vrijednosti.");
            return new EconomyRule();
        }
        
        ProgressionLevel level = progressionLevels[progressionLevel - 1];
        
        EconomyRule progressed = new EconomyRule();
        progressed.ruleName = $"Economy Rule Level {progressionLevel}";
        progressed.ruleDescription = $"Economy modifikatori za nivo {progressionLevel}";
        
        // Direktno koristi multiplikatore iz levela
        progressed.waveCompleteMoneyMultiplier = level.waveCompleteMoneyMultiplier;
        progressed.enemyKillMoneyMultiplier = level.enemyKillMoneyMultiplier;
        progressed.upgradeDiscountMultiplier = level.upgradeDiscountMultiplier;
        progressed.towerPlacementCostMultiplier = level.towerPlacementCostMultiplier;
        
        return progressed;
    }
    
    // Uklonjen jer više ne koristimo base values, već direktno level values
    
    // Pomocna metoda za kreiranje default progression levelsa
    public void CreateDefaultProgressionLevels()
    {
        progressionLevels = new ProgressionLevel[5];
        
        for (int i = 0; i < progressionLevels.Length; i++)
        {
            progressionLevels[i] = new ProgressionLevel();
            progressionLevels[i].levelDescription = $"Opis modifikatora za nivo {i + 1}";
            
            // Default vrijednosti (1f = bez promjene)
            // Ti možeš manuelno podesiti ove vrijednosti u Inspector-u
        }
    }
    
    // Getter za maksimalni nivo progresije
    public int GetMaxProgressionLevel()
    {
        return progressionLevels?.Length ?? 0;
    }
}