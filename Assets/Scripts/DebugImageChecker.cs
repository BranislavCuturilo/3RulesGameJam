using UnityEngine;

public class DebugImageChecker : MonoBehaviour
{
    void Update()
    {
        // Press F2 to check images
        if (Input.GetKeyDown(KeyCode.F2))
        {
            CheckRuleSetImages();
        }
    }
    
    void CheckRuleSetImages()
    {
        Debug.Log("=== RULE SET IMAGES CHECK ===");
        
        if (RuleManager.main == null)
        {
            Debug.LogError("RuleManager.main is null!");
            return;
        }
        
        var ruleManager = RuleManager.main;
        
        if (ruleManager.availableRuleSets == null || ruleManager.availableRuleSets.Length == 0)
        {
            Debug.LogError("No available rule sets!");
            return;
        }
        
        Debug.Log($"Found {ruleManager.availableRuleSets.Length} rule sets:");
        
        for (int i = 0; i < ruleManager.availableRuleSets.Length; i++)
        {
            var rule = ruleManager.availableRuleSets[i];
            if (rule == null)
            {
                Debug.LogWarning($"Rule set {i} is null!");
                continue;
            }
            
            Debug.Log($"Rule Set {i}: {rule.baseRuleSetName}");
            Debug.Log($"  - Has Image: {(rule.ruleSetImage != null ? "YES" : "NO")}");
            if (rule.ruleSetImage != null)
            {
                Debug.Log($"  - Image Name: {rule.ruleSetImage.name}");
            }
            
            // Check progression levels
            if (rule.progressionLevels != null && rule.progressionLevels.Length > 0)
            {
                Debug.Log($"  - Progression Levels: {rule.progressionLevels.Length}");
                for (int j = 0; j < rule.progressionLevels.Length; j++)
                {
                    var level = rule.progressionLevels[j];
                    if (level != null)
                    {
                        Debug.Log($"    Level {j}: {(!string.IsNullOrEmpty(level.levelDescription) ? "Has Description" : "No Description")}");
                    }
                    else
                    {
                        Debug.Log($"    Level {j}: NULL");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"  - No progression levels defined!");
            }
        }
        
        // Check rule images array
        if (ruleManager.ruleImages != null)
        {
            Debug.Log("\n=== RULE IMAGES CHECK ===");
            for (int i = 0; i < ruleManager.ruleImages.Length; i++)
            {
                var image = ruleManager.ruleImages[i];
                if (image != null)
                {
                    Debug.Log($"Rule Image {i}: Current Sprite: {(image.sprite != null ? image.sprite.name : "NULL")} | Enabled: {image.enabled}");
                }
                else
                {
                    Debug.Log($"Rule Image {i}: NULL");
                }
            }
        }
        else
        {
            Debug.LogWarning("Rule Images array is NULL - add it to RuleManager inspector!");
        }
        
        // Check current buttons
        if (ruleManager.optionButtons != null)
        {
            Debug.Log("\n=== BUTTON CHECK ===");
            for (int i = 0; i < ruleManager.optionButtons.Length; i++)
            {
                var button = ruleManager.optionButtons[i];
                if (button != null)
                {
                    Debug.Log($"Button {i}: {button.name} - Active: {button.gameObject.activeSelf}");
                }
                else
                {
                    Debug.Log($"Button {i}: NULL");
                }
            }
        }
        
        Debug.Log("=== END IMAGE CHECK ===");
    }
}