using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class RuleManager : MonoBehaviour
{
    public static RuleManager main;

    // UI elementi (dodaj ih u Inspectoru)
    [Header("UI Setup")]
    [SerializeField] public GameObject rulePanel; // Novi panel u sredini ekrana
    [SerializeField] public Button[] optionButtons; // 3 dugmeta za opcije
    [SerializeField] public TextMeshProUGUI[] optionTexts; // Tekstovi za svaku opciju
    [SerializeField] public UnityEngine.UI.Image[] ruleImages; // 3 UI slike koje će biti zamijenjene slikama iz rule setova

    // Rule Sets (dodaj ih u Inspectoru)
    [Header("Available Rule Sets")]
    [SerializeField] public Rule[] availableRuleSets; // Niz svih dostupnih rule setova
    
    // Trenutno odabrani rule setovi za prikaz
    private Rule[] currentOptions = new Rule[3];

    // Trenutno aktivna pravila
    [Header("Currently Applied Rule Set")]
    [SerializeField] private Rule currentlyAppliedRuleSet;
    [SerializeField] private int currentProgressionLevel = 0;
    
    // Progression tracking - koliko puta je svaki rule set odabran
    [Header("Rule Progression Tracking")]
    [SerializeField] private System.Collections.Generic.Dictionary<string, int> ruleProgressionCount = new System.Collections.Generic.Dictionary<string, int>();

    void Awake() { main = this; }

    // Pozovi ovo nakon vala (iz EnemyManager)
    public void ShowRuleOptions()
    {
        if (availableRuleSets == null || availableRuleSets.Length < 3)
        {
            Debug.LogError("RuleManager: Potrebno je minimalno 3 Rule Set-a u availableRuleSets!");
            return;
        }
        
        rulePanel.SetActive(true);
        
        // Odaberi 3 random rule set-a
        GenerateRandomRuleOptions();
        
        for (int i = 0; i < 3; i++)
        {
            if (currentOptions[i] != null && optionButtons != null && i < optionButtons.Length)
            {
                Rule currentRule = currentOptions[i];
                int progressionLevel = GetRuleProgressionLevel(currentRule);
                
                // Postavi kompletan sadržaj button-a iz ScriptableObject-a
                if (optionTexts != null && i < optionTexts.Length && optionTexts[i] != null)
                {
                    // Generiši kompletan text: naziv + opis
                    string ruleName = currentRule.GetRuleSetName(progressionLevel);
                    string displayText = currentRule.GetDisplayText(progressionLevel);
                    
                    // Ako je displayText prazan ili invalid, koristi fallback
                    if (string.IsNullOrEmpty(displayText) || displayText == "Nivo nije definisan")
                    {
                        displayText = $"Level {progressionLevel + 1}\nKonfigurišite progression level u Inspector-u";
                    }
                    
                    // Kombinuj naziv i opis
                    string fullText = $"{ruleName}\n\n{displayText}";
                    optionTexts[i].text = fullText;
                }
                
                // Postavi sliku iz ScriptableObject-a na zasebnu Image komponentu
                if (ruleImages != null && i < ruleImages.Length && ruleImages[i] != null)
                {
                    if (currentRule.ruleSetImage != null)
                    {
                        ruleImages[i].sprite = currentRule.ruleSetImage;
                        ruleImages[i].enabled = true;
                    }
                    else
                    {
                        ruleImages[i].enabled = false; // Sakrij ako nema slike
                    }
                }
                
                // Setup button click
            int index = i; // Za lambda
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => SelectOption(index));
            }
        }
    }

    private void GenerateRandomRuleOptions()
    {
        // Stvori kopiju liste dostupnih rule setova
        List<Rule> tempList = new List<Rule>(availableRuleSets);
        
        // Odaberi 3 različita rule set-a
        for (int i = 0; i < 3; i++)
        {
            if (tempList.Count > 0)
            {
                int randomIndex = Random.Range(0, tempList.Count);
                currentOptions[i] = tempList[randomIndex];
                tempList.RemoveAt(randomIndex); // Ukloni da se ne ponovi
            }
        }
    }

    private void SelectOption(int index)
    {
        if (currentOptions != null && index >= 0 && index < currentOptions.Length && currentOptions[index] != null)
        {
            Rule selectedRule = currentOptions[index];
            
            // Ažuriraj progression count
            IncrementRuleProgressionCount(selectedRule);
            
            // Spremi trenutno odabrani rule set i nivo
            currentlyAppliedRuleSet = selectedRule;
            currentProgressionLevel = GetRuleProgressionLevel(selectedRule);
            
            // Primijeni rule set sa progression levelom
            ApplyRuleSet(currentlyAppliedRuleSet, currentProgressionLevel);
            
            Debug.Log($"Applied Rule Set: {currentlyAppliedRuleSet.GetRuleSetName(currentProgressionLevel)}");
            
                        // Sakrij panel i pokreni sljedeći val
            rulePanel.SetActive(false);
            
            // Provjeri da li EnemyManager postoji prije poziva NextWave
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
        
        // NE pozivamo ResetModifiers() ovdje jer se već poziva u EnemyManager.Update()
        // ResetModifiers(); // REMOVED - causing double reset
        
        // Dobij progresirane rule verzije
        EnemyRule progressedEnemyRule = ruleSet.GetProgressedEnemyRule(progressionLevel);
        TowerRule progressedTowerRule = ruleSet.GetProgressedTowerRule(progressionLevel);
        EconomyRule progressedEconomyRule = ruleSet.GetProgressedEconomyRule(progressionLevel);
        
        // Primijeni modifikatore
        ApplyTowerModifiers(progressedTowerRule);
        
        // Spremi progresirane rule-ove za korišćenje u getter metodama
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
            
            // Provjeri da li se pravilo odnosi na sve tornjeve ili specifičan tip
            bool shouldApply = towerRule.targetTowerType == TowerType.All || 
                               DoesTowerMatchType(tower, towerRule.targetTowerType);
            
            if (shouldApply)
            {
                // Primijeni modifikatore
                tower.FireRate = tower.originalFireRate * towerRule.fireRateMultiplier;
                tower.Damage = Mathf.RoundToInt(tower.originalDamage * towerRule.damageMultiplier);
                tower.Range = tower.originalRange * towerRule.rangeMultiplier;
                
                // Ažuriraj range visually
                TowerRange towerRange = towerObj.GetComponent<TowerRange>();
                if (towerRange != null)
                {
                    towerRange.UpdateRange();
                }
                
                // Promijeni targeting mode ako je potrebno
                if (towerRule.forceTargetingMode != TargetingMode.NoChange)
                {
                    ApplyTargetingMode(tower, towerRule.forceTargetingMode);
                }
                
                // Primijeni advanced efekte na TowerEffects komponentu
                TowerEffects towerEffects = towerObj.GetComponent<TowerEffects>();
                if (towerEffects != null)
                {
                    ApplyAdvancedEffects(towerEffects, towerRule);
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
        // Resetuj sve targeting mode-ove
        tower.First = false;
        tower.Last = false;
        tower.Strongest = false;
        
        // Postavi novi mode
        switch (mode)
        {
            case TargetingMode.First: tower.First = true; break;
            case TargetingMode.Last: tower.Last = true; break;
            case TargetingMode.Strongest: tower.Strongest = true; break;
        }
    }
    
    private void ApplyAdvancedEffects(TowerEffects towerEffects, TowerRule towerRule)
    {
        // Primijeni modifikatore na tower effects
        if (towerEffects.effects != null)
        {
            foreach (var effect in towerEffects.effects)
            {
                // Modifikuj effect strength na osnovu rule-a
                if (effect.effectType == EffectType.DOT || effect.effectType == EffectType.DOT_AOE)
                {
                    effect.dotDamage = Mathf.RoundToInt(effect.dotDamage * towerRule.dotDamageMultiplier);
                }
                
                if (effect.effectType == EffectType.Slow || effect.effectType == EffectType.AOE_Slow)
                {
                    effect.effectStrength *= towerRule.slowEffectMultiplier;
                }
                
                if (effect.effectType == EffectType.Stun || effect.effectType == EffectType.AOE_Stun)
                {
                    effect.effectDuration *= towerRule.stunDurationMultiplier;
                }
                
                // Modifikuj AOE radius
                if (effect.effectType.ToString().Contains("AOE"))
                {
                    effect.effectRadius *= towerRule.aoeRadiusMultiplier;
                }
            }
        }
        
        // Modifikuj projectile speed
        if (towerEffects.useProjectile)
        {
            towerEffects.projectileSpeed *= towerRule.projectileSpeedMultiplier;
        }
    }

    // Pozovi ovo nakon sljedećeg vala da resetuješ
    public void ResetModifiers()
    {
        // Resetuj tornjeve na originalne vrijednosti
        GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");
        foreach (var towerObj in towers)
        {
            Tower tower = towerObj.GetComponent<Tower>();
            if (tower != null)
            {
                tower.FireRate = tower.originalFireRate;
                tower.Damage = tower.originalDamage;
                tower.Range = tower.originalRange;
                
                // Ažuriraj range
                TowerRange towerRange = towerObj.GetComponent<TowerRange>();
                if (towerRange != null)
                {
                    towerRange.UpdateRange();
                }
            }
        }
        
        // Resetuj trenutno primijenjene rule-ove na default (1f multipliers)
        currentlyAppliedEnemyRule = new EnemyRule();
        currentlyAppliedTowerRule = new TowerRule();
        currentlyAppliedEconomyRule = new EconomyRule();
        
        // NE postavljamo currentlyAppliedRuleSet na null jer se koristi u SelectOption
        // currentlyAppliedRuleSet = null; // REMOVED - causing NullReferenceException
        
        Debug.Log("Modifiers reset to default values");
    }
    
    // Trenutno primijenjeni progresirani rule-ovi
    private EnemyRule currentlyAppliedEnemyRule;
    private TowerRule currentlyAppliedTowerRule;
    private EconomyRule currentlyAppliedEconomyRule;
    
    // Getter metode za pristup trenutnim modifikatorima (sa progression)
    public float GetEnemySpeedMod()
    {
        return currentlyAppliedEnemyRule?.speedMultiplier ?? 1f;
    }
    
    public float GetEnemyHPMod()
    {
        return currentlyAppliedEnemyRule?.healthMultiplier ?? 1f;
    }
    
    public float GetEnemyMoneyMod()
    {
        return currentlyAppliedEnemyRule?.moneyValueMultiplier ?? 1f;
    }
    
    public float GetEconomyBonusMod()
    {
        return currentlyAppliedEconomyRule?.waveCompleteMoneyMultiplier ?? 1f;
    }
    
    public float GetEconomyMoneyMod()
    {
        return currentlyAppliedEconomyRule?.enemyKillMoneyMultiplier ?? 1f;
    }
    
    public float GetTowerPlacementCostMod()
    {
        return currentlyAppliedEconomyRule?.towerPlacementCostMultiplier ?? 1f;
    }
    
    public float GetUpgradeDiscountMod()
    {
        return currentlyAppliedEconomyRule?.upgradeDiscountMultiplier ?? 1f;
    }
    
    // Progression tracking metode
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
        
        // Ograniči na maksimalni nivo
        ruleProgressionCount[ruleSet.baseRuleSetName] = Mathf.Min(
            ruleProgressionCount[ruleSet.baseRuleSetName], 
            ruleSet.GetMaxProgressionLevel()
        );
    }
    
    // Debug metoda za provjeru progression stanja
    public void LogProgressionStatus()
    {
        Debug.Log("=== Rule Progression Status ===");
        foreach (var kvp in ruleProgressionCount)
        {
            Debug.Log($"{kvp.Key}: Level {kvp.Value + 1}");
        }
    }
}