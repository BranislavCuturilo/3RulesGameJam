using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class RuleManager : MonoBehaviour
{
    public static RuleManager main;

    // UI elementi (dodaj ih u Inspectoru)
    [SerializeField] private GameObject RulePanel; // Novi panel u sredini ekrana
    [SerializeField] private Button[] OptionButtons; // 3 dugmeta za opcije
    [SerializeField] private TextMeshProUGUI[] OptionTexts; // Tekstovi za svaku opciju
    [SerializeField] private TextMeshProUGUI[] RuleNameTexts; // Nazivi rule setova
    [SerializeField] private UnityEngine.UI.Image[] RuleImages; // Slike za rule setove

    // Rule Sets (dodaj ih u Inspectoru)
    [Header("Available Rule Sets")]
    [SerializeField] private Rule[] availableRuleSets; // Niz svih dostupnih rule setova
    
    // Trenutno odabrani rule setovi za prikaz
    private Rule[] currentOptions = new Rule[3];

    // Trenutno aktivna pravila
    [Header("Currently Applied Rule Set")]
    [SerializeField] private Rule currentlyAppliedRuleSet;

    void Awake() { main = this; }

    // Pozovi ovo nakon vala (iz EnemyManager)
    public void ShowRuleOptions()
    {
        if (availableRuleSets == null || availableRuleSets.Length < 3)
        {
            Debug.LogError("RuleManager: Potrebno je minimalno 3 Rule Set-a u availableRuleSets!");
            return;
        }
        
        RulePanel.SetActive(true);
        
        // Odaberi 3 random rule set-a
        GenerateRandomRuleOptions();
        
        for (int i = 0; i < 3; i++)
        {
            if (currentOptions[i] != null)
            {
                // Postavi naziv rule seta
                if (RuleNameTexts != null && i < RuleNameTexts.Length && RuleNameTexts[i] != null)
                {
                    RuleNameTexts[i].text = currentOptions[i].ruleSetName;
                }
                
                // Postavi opis pravila
                if (OptionTexts != null && i < OptionTexts.Length && OptionTexts[i] != null)
                {
                    OptionTexts[i].text = currentOptions[i].GetDisplayText();
                }
                
                // Postavi sliku
                if (RuleImages != null && i < RuleImages.Length && RuleImages[i] != null)
                {
                    if (currentOptions[i].ruleSetImage != null)
                    {
                        RuleImages[i].sprite = currentOptions[i].ruleSetImage;
                        RuleImages[i].enabled = true;
                    }
                    else
                    {
                        RuleImages[i].enabled = false;
                    }
                }
                
                int index = i; // Za lambda
                OptionButtons[i].onClick.RemoveAllListeners();
                OptionButtons[i].onClick.AddListener(() => SelectOption(index));
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
        if (currentOptions[index] != null)
        {
            // Spremi trenutno odabrani rule set
            currentlyAppliedRuleSet = currentOptions[index];
            
            // Primijeni rule set
            ApplyRuleSet(currentlyAppliedRuleSet);
            
            Debug.Log($"Applied Rule Set: {currentlyAppliedRuleSet.ruleSetName}");
        }

        RulePanel.SetActive(false);
        EnemyManager.main.NextWave(); // Pokreni sljedeći val
    }

    public void ApplyRuleSet(Rule ruleSet)
    {
        if (ruleSet == null) return;
        
        // Resetuj prethodne modifikatore
        ResetModifiers();
        
        // Primijeni tower modifikatore
        ApplyTowerModifiers(ruleSet.towerRule);
        
        Debug.Log($"Applied rule set: {ruleSet.ruleSetName}");
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
        
        currentlyAppliedRuleSet = null;
        Debug.Log("Modifiers reset to default values");
    }
    
    // Getter metode za pristup trenutnim modifikatorima (za kompatibilnost sa postojećim kodom)
    public float GetEnemySpeedMod()
    {
        return currentlyAppliedRuleSet?.enemyRule.speedMultiplier ?? 1f;
    }
    
    public float GetEnemyHPMod()
    {
        return currentlyAppliedRuleSet?.enemyRule.healthMultiplier ?? 1f;
    }
    
    public float GetEnemyMoneyMod()
    {
        return currentlyAppliedRuleSet?.enemyRule.moneyValueMultiplier ?? 1f;
    }
    
    public float GetEconomyBonusMod()
    {
        return currentlyAppliedRuleSet?.economyRule.waveCompleteMoneyMultiplier ?? 1f;
    }
    
    public float GetEconomyMoneyMod()
    {
        return currentlyAppliedRuleSet?.economyRule.enemyKillMoneyMultiplier ?? 1f;
    }
    
    public float GetTowerPlacementCostMod()
    {
        return currentlyAppliedRuleSet?.economyRule.towerPlacementCostMultiplier ?? 1f;
    }
    
    public float GetUpgradeDiscountMod()
    {
        return currentlyAppliedRuleSet?.economyRule.upgradeDiscountMultiplier ?? 1f;
    }
}