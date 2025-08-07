using UnityEngine;

public class DebugRuleManager : MonoBehaviour
{
    [Header("Debug Controls")]
    public KeyCode debugKey = KeyCode.F1;
    public bool enableDebugLogs = true;
    
    void Update()
    {
        if (Input.GetKeyDown(debugKey))
        {
            DebugRuleManagerState();
        }
    }
    
    private void DebugRuleManagerState()
    {
        if (RuleManager.main == null)
        {
            Debug.LogError("RuleManager.main is NULL!");
            return;
        }
        
        Debug.Log("=== RULE MANAGER DEBUG INFO ===");
        
        // Check available rule sets
        if (RuleManager.main.availableRuleSets == null)
        {
            Debug.LogError("availableRuleSets is NULL!");
        }
        else
        {
            Debug.Log($"Available Rule Sets: {RuleManager.main.availableRuleSets.Length}");
            for (int i = 0; i < RuleManager.main.availableRuleSets.Length; i++)
            {
                if (RuleManager.main.availableRuleSets[i] == null)
                {
                    Debug.LogWarning($"Rule Set at index {i} is NULL!");
                }
                else
                {
                    var rule = RuleManager.main.availableRuleSets[i];
                    Debug.Log($"Rule {i}: {rule.baseRuleSetName} (Levels: {rule.GetMaxProgressionLevel()})");
                    
                    // Check progression levels
                    if (rule.progressionLevels == null)
                    {
                        Debug.LogWarning($"Rule {rule.baseRuleSetName} has NULL progression levels!");
                    }
                    else
                    {
                        for (int j = 0; j < rule.progressionLevels.Length; j++)
                        {
                            if (rule.progressionLevels[j] == null)
                            {
                                Debug.LogWarning($"Rule {rule.baseRuleSetName} Level {j} is NULL!");
                            }
                            else
                            {
                                Debug.Log($"  Level {j}: {rule.progressionLevels[j].levelDescription}");
                            }
                        }
                    }
                }
            }
        }
        
        // Check UI elements
        var ruleManager = RuleManager.main;
        Debug.Log("UI Elements Check:");
        Debug.Log($"- Rule Panel: {(ruleManager.rulePanel != null ? "OK" : "NULL")}");
        Debug.Log($"- Option Buttons: {(ruleManager.optionButtons != null ? ruleManager.optionButtons.Length.ToString() : "NULL")}");
        Debug.Log($"- Option Texts: {(ruleManager.optionTexts != null ? ruleManager.optionTexts.Length.ToString() : "NULL")}");
        
        // Check progression tracking
        ruleManager.LogProgressionStatus();
        
        Debug.Log("=== END DEBUG INFO ===");
    }
}