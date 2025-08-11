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
    [SerializeField] public TextMeshProUGUI[] optionTexts; // (legacy) kombinovani naziv+opis
    [SerializeField] public TextMeshProUGUI[] optionNameTexts; // naziv
    [SerializeField] public TextMeshProUGUI[] optionDescriptionTexts; // opis
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
        
        // Ne prikazuj panel ako je igra gotova
        if (Player.main != null && Player.main.IsGameOver)
        {
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
                int currentLevel = GetRuleProgressionLevel(currentRule); // 0-based current usage count
                int maxLevel = Mathf.Max(1, currentRule.GetMaxProgressionLevel());
                int displayLevel = Mathf.Clamp(currentLevel + 1, 1, maxLevel); // preview next level that will be applied if selected
                
                // Pripremi naziv i opis
                // Show name for the same level index: GetRuleSetName expects 0-based input internally adds +1
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
                // Fallback na legacy kombinovani text
                if (!usedSplit && optionTexts != null && i < optionTexts.Length && optionTexts[i] != null)
                {
                    optionTexts[i].text = $"{ruleName}\n\n{displayText}";
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
                // FireRate je brzina (metci/sek is implicit); naš Field "FireRate" u Tower se ponaša kao cooldown (sekundi po metku)?
                // U tvojoj igri je FireRate vrednost koja određuje koliko brzo puni cooldown (CoolDown += 1f * deltaTime; puca kada CoolDown >= FireRate),
                // što znači: manji broj = brže. Dakle multiplikator > 1 znači brže ⇒ period = original / multiplier.
                float fireRateRatio = Mathf.Max(0.0001f, towerRule.fireRateMultiplier);
                tower.FireRate = tower.originalFireRate / fireRateRatio;
                tower.Damage = Mathf.RoundToInt(tower.originalDamage * towerRule.damageMultiplier);
                tower.Range = Mathf.Max(3f, tower.originalRange * towerRule.rangeMultiplier); // min 3
                
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
                // Napomena: Više ne mutiramo trajno TowerEffects vrijednosti ovdje.
                // Efekti i projectile parametri će se skalirati dinamički pri pucu (TowerShotBuilder/Projectile).
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
    
    // Uklonjeno trajno mijenjanje TowerEffects; efekti se skaliraju pri izgradnji metka

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

    public float GetEnemyQuantityMod()
    {
        return currentlyAppliedEnemyRule?.quantityMultiplier ?? 1f;
    }
    
    // Koliko damage-a neprijatelj nanosi kada "procura" (stigne do kraja)
    // Ako je definisan fiksni damage u pravilima za ovaj krug, vrati ga (>=0),
    // inače vrati -1 kao signal da se koristi vrijednost sa samog neprijatelja.
    public int GetEnemyLeakDamageOverride()
    {
        if (currentlyAppliedEnemyRule == null) return -1;
        // Fixed override has priority
        if (currentlyAppliedEnemyRule.useFixedLeakDamage)
        {
            return Mathf.Max(1, currentlyAppliedEnemyRule.fixedLeakDamage);
        }
        // Otherwise return scaled value marker -1, caller can multiply local LeakDamage
        return -1;
    }
    
    public float GetEnemyMoneyMod()
    {
        return currentlyAppliedEnemyRule?.moneyValueMultiplier ?? 1f;
    }

    // Leak dmg multiplier (applies when no fixed override is used)
    public float GetEnemyLeakDamageMultiplier()
    {
        return currentlyAppliedEnemyRule?.leakDamageMultiplier ?? 1f;
    }

    // Fixed enemy count override for a wave
    public int GetFixedEnemyCountOverride()
    {
        if (currentlyAppliedEnemyRule != null && currentlyAppliedEnemyRule.useFixedEnemyCount)
        {
            return Mathf.Max(0, currentlyAppliedEnemyRule.fixedEnemyCount);
        }
        return -1; // no override
    }

    // Spawn delay override for current wave
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

    // Getter za aktivni TowerRule (za TowerShotBuilder)
    public TowerRule GetCurrentlyAppliedTowerRule()
    {
        return currentlyAppliedTowerRule;
    }

    // Enemy fixed HP override for wave
    public int GetFixedEnemyHealthOverride()
    {
        if (currentlyAppliedEnemyRule != null && currentlyAppliedEnemyRule.useFixedEnemyHealth)
        {
            return Mathf.Max(1, currentlyAppliedEnemyRule.fixedEnemyHealth);
        }
        return -1;
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