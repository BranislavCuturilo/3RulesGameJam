using UnityEngine;

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
    public string ruleSetName = "New Rule Set";
    [TextArea(2, 3)]
    public string ruleSetDescription = "Opisuje set od 3 pravila";
    
    [Header("Rule Set Image")]
    public Sprite ruleSetImage; // Slika za prikaz u UI
    
    [Header("Individual Rules")]
    public EnemyRule enemyRule = new EnemyRule();
    public TowerRule towerRule = new TowerRule();
    public EconomyRule economyRule = new EconomyRule();
    
    [Header("Display Options")]
    public bool useCustomDisplay = false;
    [TextArea(4, 8)]
    public string customDisplayText;
    
    public string GetDisplayText()
    {
        if (useCustomDisplay && !string.IsNullOrEmpty(customDisplayText))
        {
            return customDisplayText;
        }
        
        string enemyText = enemyRule.targetEnemyType == EnemyType.All ? 
            enemyRule.ruleDescription : 
            $"{enemyRule.ruleDescription} ({enemyRule.targetEnemyType})";
            
        string towerText = towerRule.targetTowerType == TowerType.All ? 
            towerRule.ruleDescription : 
            $"{towerRule.ruleDescription} ({towerRule.targetTowerType})";
            
        return $"🔴 {enemyText}\n🔵 {towerText}\n🟡 {economyRule.ruleDescription}";
    }
    
    public void ApplyRules()
    {
        if (RuleManager.main != null)
        {
            RuleManager.main.ApplyRuleSet(this);
        }
    }
}