using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class TowerUpgrade : MonoBehaviour
{
    [System.Serializable]
    class Level
    {
        public int Damage = 25;
        public float FireRate = 8f;
        public float Range = 1f;
        public int Cost = 100;

        [Header("Effect Multipliers (apply to base TowerEffects)")]
        public float effectRadiusMultiplier = 1f;
        public float effectDurationMultiplier = 1f;
        public float effectStrengthMultiplier = 1f;
        public float dotDamageMultiplier = 1f;
        public float projectileSpeedMultiplier = 1f;
    }

    [SerializeField] private Level[] Levels = new Level[3];
    [NonSerialized] public int CurrentLevel = 0;
    [NonSerialized] public string CurrentCost;

    private Tower Tower;
    [SerializeField] private TowerRange TowerRange;

    [Header("Visuals by Level")]
    [Tooltip("Assign one GameObject per level visual (e.g., Sprite1, Sprite2, Sprite3). Index 0 is base (level 1), 1 is after first upgrade (level 2), etc.")]
    [SerializeField] private GameObject[] levelSprites;

    void Awake()
    {
        Tower = GetComponent<Tower>();
        UpdateCostDisplay();
        UpdateVisuals();
    }

    public void Upgrade()
    {
        int nextIndex = CurrentLevel + 1;
        if (Levels == null) return;
        if(nextIndex < Levels.Length)
        {
            int adjustedCost = Mathf.CeilToInt(Levels[nextIndex].Cost * RuleManager.main.GetUpgradeDiscountMod());
            if (adjustedCost <= Player.main.Money)
            {
                // 1) Update tower base stats to next level
                Tower.Range = Levels[nextIndex].Range;
                Tower.FireRate = Levels[nextIndex].FireRate;
                Tower.Damage = Levels[nextIndex].Damage;

                // Ensure future resets keep upgraded stats as base
                Tower.originalRange = Tower.Range;
                Tower.originalFireRate = Tower.FireRate;
                Tower.originalDamage = Tower.Damage;

                // 2) Update TowerEffects base stats using multipliers for next level
                var towerEffects = GetComponent<TowerEffects>();
                if (towerEffects != null)
                {
                    // Projectile speed base
                    towerEffects.projectileSpeed *= Levels[nextIndex].projectileSpeedMultiplier;

                    // Per-effect base mods
                    if (towerEffects.effects != null)
                    {
                        foreach (var e in towerEffects.effects)
                        {
                            if (e == null) continue;
                            e.effectRadius *= Levels[nextIndex].effectRadiusMultiplier;
                            e.effectDuration *= Levels[nextIndex].effectDurationMultiplier;
                            e.effectStrength *= Levels[nextIndex].effectStrengthMultiplier;
                            e.dotDamage = Mathf.CeilToInt(e.dotDamage * Levels[nextIndex].dotDamageMultiplier);
                        }
                    }
                }

                Player.main.Money -= adjustedCost;
                
                TowerRange.UpdateRange();

                CurrentLevel = nextIndex;

                UpdateCostDisplay();
                UpdateVisuals();
            }
        }
    }
    
    public void UpdateCostDisplay()
    {
        if (Levels == null || Levels.Length == 0)
        {
            CurrentCost = string.Empty;
            return;
        }

        int nextIndex = CurrentLevel + 1;
        if(nextIndex >= Levels.Length)
        {
            CurrentCost = "MAX";
        }
        else
        {
            int adjustedCost = Mathf.CeilToInt(Levels[nextIndex].Cost * RuleManager.main.GetUpgradeDiscountMod());
            CurrentCost = adjustedCost.ToString();
        }
    }

    private void UpdateVisuals()
    {
        if (levelSprites == null || levelSprites.Length == 0) return;

        int activeIndex = Mathf.Clamp(CurrentLevel, 0, levelSprites.Length - 1);
        for (int i = 0; i < levelSprites.Length; i++)
        {
            GameObject spriteObj = levelSprites[i];
            if (spriteObj == null) continue;
            bool shouldBeActive = i == activeIndex;
            if (spriteObj.activeSelf != shouldBeActive)
            {
                spriteObj.SetActive(shouldBeActive);
            }
        }
    }
}
