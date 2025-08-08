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

    void Awake()
    {
        Tower = GetComponent<Tower>();
        UpdateCostDisplay();
    }

    public void Upgrade()
    {
        int adjustedCost = Mathf.CeilToInt(Levels[CurrentLevel].Cost * RuleManager.main.GetUpgradeDiscountMod());
        if(CurrentLevel < Levels.Length && adjustedCost <= Player.main.Money)
        {
            // 1) Update tower base stats
            Tower.Range = Levels[CurrentLevel].Range;
            Tower.FireRate = Levels[CurrentLevel].FireRate;
            Tower.Damage = Levels[CurrentLevel].Damage;

            // Ensure future resets keep upgraded stats as base
            Tower.originalRange = Tower.Range;
            Tower.originalFireRate = Tower.FireRate;
            Tower.originalDamage = Tower.Damage;

            // 2) Update TowerEffects base stats using multipliers
            var towerEffects = GetComponent<TowerEffects>();
            if (towerEffects != null)
            {
                // Projectile speed base
                towerEffects.projectileSpeed *= Levels[CurrentLevel].projectileSpeedMultiplier;

                // Per-effect base mods
                if (towerEffects.effects != null)
                {
                    foreach (var e in towerEffects.effects)
                    {
                        if (e == null) continue;
                        e.effectRadius *= Levels[CurrentLevel].effectRadiusMultiplier;
                        e.effectDuration *= Levels[CurrentLevel].effectDurationMultiplier;
                        e.effectStrength *= Levels[CurrentLevel].effectStrengthMultiplier;
                        e.dotDamage = Mathf.CeilToInt(e.dotDamage * Levels[CurrentLevel].dotDamageMultiplier);
                    }
                }
            }

            Player.main.Money -= adjustedCost;
            
            TowerRange.UpdateRange();

            CurrentLevel++;

            UpdateCostDisplay();
        }
    }
    
    public void UpdateCostDisplay()
    {
        if(CurrentLevel >= Levels.Length)
        {
            CurrentCost = "MAX";
        }
        else
        {
            int adjustedCost = Mathf.CeilToInt(Levels[CurrentLevel].Cost * RuleManager.main.GetUpgradeDiscountMod());
            CurrentCost = adjustedCost.ToString();
        }
    }
}
