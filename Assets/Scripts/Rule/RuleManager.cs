using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class RuleManager : MonoBehaviour
{
    public static RuleManager main;

    [Header("UI Setup")]
    [SerializeField] public GameObject rulePanel; 
    [SerializeField] public Button[] optionButtons; 
    [SerializeField] public TextMeshProUGUI[] optionTexts; 
    [SerializeField] public TextMeshProUGUI[] optionNameTexts; 
    [SerializeField] public TextMeshProUGUI[] optionDescriptionTexts; 
    [SerializeField] public UnityEngine.UI.Image[] ruleImages; 

    [Header("Available Rule Sets")]
    [SerializeField] public Rule[] availableRuleSets; 
    
    private Rule[] currentOptions = new Rule[3];

    [Header("Currently Applied Rule Set")]
    [SerializeField] private Rule currentlyAppliedRuleSet;
    [SerializeField] private int currentProgressionLevel = 0;
    
    [Header("Rule Progression Tracking")]
    [SerializeField] private System.Collections.Generic.Dictionary<string, int> ruleProgressionCount = new System.Collections.Generic.Dictionary<string, int>();

    void Awake() { main = this; }

    public void ShowRuleOptions()
    {
        if (availableRuleSets == null || availableRuleSets.Length < 3)
        {
            Debug.LogError("RuleManager: Potrebno je minimalno 3 Rule Set-a u availableRuleSets!");
            return;
        }
        
        if (Player.main != null && Player.main.IsGameOver)
        {
            return;
        }
        rulePanel.SetActive(true);
        
        GenerateRandomRuleOptions();
        
        for (int i = 0; i < 3; i++)
        {
            if (currentOptions[i] != null && optionButtons != null && i < optionButtons.Length)
            {
                Rule currentRule = currentOptions[i];
                int currentLevel = GetRuleProgressionLevel(currentRule); 
                int maxLevel = Mathf.Max(1, currentRule.GetMaxProgressionLevel());
                int displayLevel = Mathf.Clamp(currentLevel + 1, 1, maxLevel); 
                
                string ruleName = currentRule.GetRuleSetName(displayLevel - 1);
                string displayText = currentRule.GetDisplayText(displayLevel);
                if (string.IsNullOrEmpty(displayText) || displayText == "Nivo nije definisan")
                {
                    displayText = $"Level {displayLevel}\nKonfigurišite progression level u Inspector-u";
                }

                bool usedSplit = false;
                if (optionNameTexts != null && i < optionNameTexts.Length && optionNameTexts[i] != null)
                {
                    optionNameTexts[i].text = ruleName;
                    usedSplit = true;
                }
                if (optionDescriptionTexts != null && i < optionDescriptionTexts.Length && optionDescriptionTexts[i] != null)
                {
                    optionDescriptionTexts[i].text = displayText;
                    usedSplit = true;
                }
                if (!usedSplit && optionTexts != null && i < optionTexts.Length && optionTexts[i] != null)
                {
                    optionTexts[i].text = $"{ruleName}\n\n{displayText}";
                }
                
                if (ruleImages != null && i < ruleImages.Length && ruleImages[i] != null)
                {
                    if (currentRule.ruleSetImage != null)
                    {
                        ruleImages[i].sprite = currentRule.ruleSetImage;
                        ruleImages[i].enabled = true;
                    }
                    else
                    {
                        ruleImages[i].enabled = false; 
                    }
                }
                
                int index = i; 
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => SelectOption(index));
            }
        }
    }

    private void GenerateRandomRuleOptions()
    {
        List<Rule> tempList = new List<Rule>(availableRuleSets);
        
        for (int i = 0; i < 3; i++)
        {
            if (tempList.Count > 0)
            {
                int randomIndex = Random.Range(0, tempList.Count);
                currentOptions[i] = tempList[randomIndex];
                tempList.RemoveAt(randomIndex); 
            }
        }
    }

    private void SelectOption(int index)
    {
        if (currentOptions != null && index >= 0 && index < currentOptions.Length && currentOptions[index] != null)
        {
            Rule selectedRule = currentOptions[index];
            
            IncrementRuleProgressionCount(selectedRule);
            
            currentlyAppliedRuleSet = selectedRule;
            currentProgressionLevel = GetRuleProgressionLevel(selectedRule);
            
            ApplyRuleSet(currentlyAppliedRuleSet, currentProgressionLevel);
            
            Debug.Log($"Applied Rule Set: {currentlyAppliedRuleSet.GetRuleSetName(currentProgressionLevel)}");
            
            rulePanel.SetActive(false);
            
            if (EnemyManager.main != null)
            {
                EnemyManager.main.NextWave();
            }
            else
            {
                Debug.LogError("EnemyManager.main is null - cannot start next wave!");
            }
        }
        else
        {
            Debug.LogError($"SelectOption called with invalid index {index} or null rule at that index");
        }
    }

    public void ApplyRuleSet(Rule ruleSet, int progressionLevel = 0)
    {
        if (ruleSet == null) return;
        
        ResetModifiers(); 
        
        EnemyRule progressedEnemyRule = ruleSet.GetProgressedEnemyRule(progressionLevel);
        TowerRule progressedTowerRule = ruleSet.GetProgressedTowerRule(progressionLevel);
        EconomyRule progressedEconomyRule = ruleSet.GetProgressedEconomyRule(progressionLevel);
        
        ApplyTowerModifiers(progressedTowerRule);
        
        currentlyAppliedEnemyRule = progressedEnemyRule;
        currentlyAppliedTowerRule = progressedTowerRule;
        currentlyAppliedEconomyRule = progressedEconomyRule;
        
        Debug.Log($"Applied rule set: {ruleSet.GetRuleSetName(progressionLevel)} with manual progression level");
    }
    
    private void ApplyTowerModifiers(TowerRule towerRule)
    {
        GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");
        foreach (var towerObj in towers)
        {
            Tower tower = towerObj.GetComponent<Tower>();
            if (tower == null) continue;
            
            bool shouldApply = towerRule.targetTowerType == TowerType.All || 
                               DoesTowerMatchType(tower, towerRule.targetTowerType);
            
            if (shouldApply)
            {
                float fireRateRatio = Mathf.Max(0.0001f, towerRule.fireRateMultiplier);
                tower.FireRate = tower.originalFireRate / fireRateRatio;
                tower.Damage = Mathf.RoundToInt(tower.originalDamage * towerRule.damageMultiplier);
                tower.Range = Mathf.Max(3f, tower.originalRange * towerRule.rangeMultiplier); // min 3
                
                TowerRange towerRange = towerObj.GetComponent<TowerRange>();
                if (towerRange != null)
                {
                    towerRange.UpdateRange();
                }
                
                if (towerRule.forceTargetingMode != TargetingMode.NoChange)
                {
                    ApplyTargetingMode(tower, towerRule.forceTargetingMode);
                }
            }
        }
    }
    
    private bool DoesTowerMatchType(Tower tower, TowerType targetType)
    {
        string towerName = tower.TowerName.Replace("(Clone)", "").Trim();
        
        switch (targetType)
        {
            case TowerType.CannonTower: return towerName.Contains("Cannon");
            case TowerType.SniperTower: return towerName.Contains("Sniper");
            case TowerType.MachineGunTower: return towerName.Contains("MachineGun") || towerName.Contains("Machine Gun");
            case TowerType.ShotgunTower: return towerName.Contains("Shotgun") || towerName.Contains("ShotGun");
            default: return true;
        }
    }
    
    private void ApplyTargetingMode(Tower tower, TargetingMode mode)
    {
        tower.First = false;
        tower.Last = false;
        tower.Strongest = false;
        
        switch (mode)
        {
            case TargetingMode.First: tower.First = true; break;
            case TargetingMode.Last: tower.Last = true; break;
            case TargetingMode.Strongest: tower.Strongest = true; break;
        }
    }
    
    public void ResetModifiers()
    {
        GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");
        foreach (var towerObj in towers)
        {
            Tower tower = towerObj.GetComponent<Tower>();
            if (tower != null)
            {
                tower.FireRate = tower.originalFireRate;
                tower.Damage = tower.originalDamage;
                tower.Range = tower.originalRange;
                
                TowerRange towerRange = towerObj.GetComponent<TowerRange>();
                if (towerRange != null)
                {
                    towerRange.UpdateRange();
                }
            }
        }
        
        currentlyAppliedEnemyRule = new EnemyRule();
        currentlyAppliedTowerRule = new TowerRule();
        currentlyAppliedEconomyRule = new EconomyRule();
        
        Debug.Log("Modifiers reset to default values");
    }
    
    private EnemyRule currentlyAppliedEnemyRule;
    private TowerRule currentlyAppliedTowerRule;
    private EconomyRule currentlyAppliedEconomyRule;
    
    public float GetEnemySpeedMod()
    {
        return currentlyAppliedEnemyRule?.speedMultiplier ?? 1f;
    }
    
    public float GetEnemyHPMod()
    {
        return currentlyAppliedEnemyRule?.healthMultiplier ?? 1f;
    }

    public float GetEnemyQuantityMod()
    {
        return currentlyAppliedEnemyRule?.quantityMultiplier ?? 1f;
    }
    
    public int GetEnemyLeakDamageOverride()
    {
        if (currentlyAppliedEnemyRule == null) return -1;
        if (currentlyAppliedEnemyRule.useFixedLeakDamage)
        {
            return Mathf.Max(1, currentlyAppliedEnemyRule.fixedLeakDamage);
        }
        return -1;
    }
    
    public float GetEnemyMoneyMod()
    {
        return currentlyAppliedEnemyRule?.moneyValueMultiplier ?? 1f;
    }

    public float GetEnemyLeakDamageMultiplier()
    {
        return currentlyAppliedEnemyRule?.leakDamageMultiplier ?? 1f;
    }

    public int GetFixedEnemyCountOverride()
    {
        if (currentlyAppliedEnemyRule != null && currentlyAppliedEnemyRule.useFixedEnemyCount)
        {
            return Mathf.Max(0, currentlyAppliedEnemyRule.fixedEnemyCount);
        }
        return -1; 
    }

    public bool TryGetSpawnDelayOverride(out float minDelay, out float maxDelay)
    {
        minDelay = 0f; maxDelay = 0f;
        if (currentlyAppliedEnemyRule != null && currentlyAppliedEnemyRule.useFixedSpawnDelay)
        {
            minDelay = Mathf.Max(0.01f, Mathf.Min(currentlyAppliedEnemyRule.fixedSpawnDelayMin, currentlyAppliedEnemyRule.fixedSpawnDelayMax));
            maxDelay = Mathf.Max(minDelay, Mathf.Max(currentlyAppliedEnemyRule.fixedSpawnDelayMin, currentlyAppliedEnemyRule.fixedSpawnDelayMax));
            return true;
        }
        return false;
    }
    
    public float GetEconomyBonusMod()
    {
        return currentlyAppliedEconomyRule?.waveCompleteMoneyMultiplier ?? 1f;
    }
    
    public float GetEconomyMoneyMod()
    {
        return currentlyAppliedEconomyRule?.enemyKillMoneyMultiplier ?? 1f;
    }

    public int GetFixedWaveCompleteMoney()
    {
        if (currentlyAppliedEconomyRule != null && currentlyAppliedEconomyRule.useFixedWaveCompleteMoney)
        {
            return Mathf.Max(0, currentlyAppliedEconomyRule.fixedWaveCompleteMoney);
        }
        return -1;
    }

    public int GetFixedEnemyKillMoney()
    {
        if (currentlyAppliedEconomyRule != null && currentlyAppliedEconomyRule.useFixedEnemyKillMoney)
        {
            return Mathf.Max(0, currentlyAppliedEconomyRule.fixedEnemyKillMoney);
        }
        return -1;
    }
    
    public float GetTowerPlacementCostMod()
    {
        return currentlyAppliedEconomyRule?.towerPlacementCostMultiplier ?? 1f;
    }
    
    public float GetUpgradeDiscountMod()
    {
        return currentlyAppliedEconomyRule?.upgradeDiscountMultiplier ?? 1f;
    }

    public TowerRule GetCurrentlyAppliedTowerRule()
    {
        return currentlyAppliedTowerRule;
    }

    public int GetFixedEnemyHealthOverride()
    {
        if (currentlyAppliedEnemyRule != null && currentlyAppliedEnemyRule.useFixedEnemyHealth)
        {
            return Mathf.Max(1, currentlyAppliedEnemyRule.fixedEnemyHealth);
        }
        return -1;
    }
    
    private int GetRuleProgressionLevel(Rule ruleSet)
    {
        if (ruleSet == null) return 0;
        
        if (ruleProgressionCount.TryGetValue(ruleSet.baseRuleSetName, out int count))
        {
            return Mathf.Min(count, ruleSet.GetMaxProgressionLevel());
        }
        return 0;
    }
    
    private void IncrementRuleProgressionCount(Rule ruleSet)
    {
        if (ruleSet == null) return;
        
        if (ruleProgressionCount.ContainsKey(ruleSet.baseRuleSetName))
        {
            ruleProgressionCount[ruleSet.baseRuleSetName]++;
        }
        else
        {
            ruleProgressionCount[ruleSet.baseRuleSetName] = 1;
        }
        
        ruleProgressionCount[ruleSet.baseRuleSetName] = Mathf.Min(
            ruleProgressionCount[ruleSet.baseRuleSetName], 
            ruleSet.GetMaxProgressionLevel()
        );
    }
    
    public void LogProgressionStatus()
    {
        Debug.Log("=== Rule Progression Status ===");
        foreach (var kvp in ruleProgressionCount)
        {
            Debug.Log($"{kvp.Key}: Level {kvp.Value + 1}");
        }
    }
}