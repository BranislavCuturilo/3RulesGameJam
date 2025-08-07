using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(Rule))]
public class RuleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Rule rule = (Rule)target;
        
        // Draw default inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Progression Tools", EditorStyles.boldLabel);
        
        // Button za kreiranje default progression levelsa
        if (GUILayout.Button("Create Default Progression Levels"))
        {
            rule.CreateDefaultProgressionLevels();
            EditorUtility.SetDirty(rule);
        }
        
        // Button za copy/paste progression levels
        if (rule.progressionLevels != null && rule.progressionLevels.Length > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Setup Examples", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Setup: Easy Progression (10-20-30-40-50%)"))
            {
                SetupEasyProgression(rule);
                EditorUtility.SetDirty(rule);
            }
            
            if (GUILayout.Button("Setup: Hard Progression (20-50-80-120-150%)"))
            {
                SetupHardProgression(rule);
                EditorUtility.SetDirty(rule);
            }
            
            if (GUILayout.Button("Setup: Extreme Progression (50-100-200-300-500%)"))
            {
                SetupExtremeProgression(rule);
                EditorUtility.SetDirty(rule);
            }
        }
        
        // Prikaz progression preview
        if (rule.progressionLevels != null && rule.progressionLevels.Length > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Progression Preview", EditorStyles.boldLabel);
            
            for (int i = 0; i < rule.progressionLevels.Length; i++)
            {
                if (rule.progressionLevels[i] != null)
                {
                    string romanNumeral = GetRomanNumeral(i + 1);
                    EditorGUILayout.LabelField($"{rule.baseRuleSetName} {romanNumeral}: {rule.progressionLevels[i].levelDescription}");
                }
            }
        }
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
    
    private void SetupEasyProgression(Rule rule)
    {
        float[] factors = { 1.1f, 1.2f, 1.3f, 1.4f, 1.5f }; // 10%, 20%, 30%, 40%, 50%
        
        for (int i = 0; i < rule.progressionLevels.Length && i < factors.Length; i++)
        {
            if (rule.progressionLevels[i] == null)
                rule.progressionLevels[i] = new ProgressionLevel();
                
            var level = rule.progressionLevels[i];
            level.levelDescription = $"Lagano povećanje težine za {(factors[i] - 1f) * 100:F0}%";
            
            // Enemy rules postaju teža
            level.enemySpeedMultiplier = factors[i];
            level.enemyHealthMultiplier = factors[i];
            
            // Tower rules postaju gora (obrnuto)
            level.towerFireRateMultiplier = 2f - factors[i]; // 1.9, 1.8, 1.7, 1.6, 1.5
            level.towerDamageMultiplier = 2f - factors[i];
            
            // Economy rules postaju gora
            level.waveCompleteMoneyMultiplier = 2f - factors[i];
            level.enemyKillMoneyMultiplier = 2f - factors[i];
        }
    }
    
    private void SetupHardProgression(Rule rule)
    {
        float[] factors = { 1.2f, 1.5f, 1.8f, 2.2f, 2.5f }; // 20%, 50%, 80%, 120%, 150%
        
        for (int i = 0; i < rule.progressionLevels.Length && i < factors.Length; i++)
        {
            if (rule.progressionLevels[i] == null)
                rule.progressionLevels[i] = new ProgressionLevel();
                
            var level = rule.progressionLevels[i];
            level.levelDescription = $"Težka progresija sa {(factors[i] - 1f) * 100:F0}% povećanjem";
            
            // Enemy rules postaju mnogo teža
            level.enemySpeedMultiplier = factors[i];
            level.enemyHealthMultiplier = factors[i];
            level.enemyDamageMultiplier = factors[i];
            
            // Tower rules postaju mnogo gora
            level.towerFireRateMultiplier = 2f - factors[i]; 
            level.towerDamageMultiplier = 2f - factors[i];
            level.towerRangeMultiplier = 2f - factors[i];
            
            // Economy rules postaju mnogo gora
            level.waveCompleteMoneyMultiplier = 2f - factors[i];
            level.enemyKillMoneyMultiplier = 2f - factors[i];
            level.towerPlacementCostMultiplier = factors[i];
        }
    }
    
    private void SetupExtremeProgression(Rule rule)
    {
        float[] factors = { 1.5f, 2f, 3f, 4f, 5f }; // 50%, 100%, 200%, 300%, 500%
        
        for (int i = 0; i < rule.progressionLevels.Length && i < factors.Length; i++)
        {
            if (rule.progressionLevels[i] == null)
                rule.progressionLevels[i] = new ProgressionLevel();
                
            var level = rule.progressionLevels[i];
            level.levelDescription = $"EKSTREMNA progresija sa {(factors[i] - 1f) * 100:F0}% povećanjem";
            
            // Svi enemy efecti ekstremni
            level.enemySpeedMultiplier = factors[i];
            level.enemyHealthMultiplier = factors[i];
            level.enemyDamageMultiplier = factors[i];
            level.enemyQuantityMultiplier = 1f + (factors[i] - 1f) * 0.5f; // Sporiji rast quantity
            
            // Svi tower efecti ekstremno gori
            level.towerFireRateMultiplier = Mathf.Max(0.1f, 2f - factors[i]);
            level.towerDamageMultiplier = Mathf.Max(0.1f, 2f - factors[i]);
            level.towerRangeMultiplier = Mathf.Max(0.1f, 2f - factors[i]);
            level.towerPlacementCostMultiplier = factors[i];
            
            // Svi economy efecti ekstremno gori
            level.waveCompleteMoneyMultiplier = Mathf.Max(0.1f, 2f - factors[i]);
            level.enemyKillMoneyMultiplier = Mathf.Max(0.1f, 2f - factors[i]);
            level.upgradeDiscountMultiplier = Mathf.Max(0.1f, 2f - factors[i]);
        }
    }
}
#endif