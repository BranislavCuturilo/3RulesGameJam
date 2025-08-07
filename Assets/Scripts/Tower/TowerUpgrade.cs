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
        int adjustedCost = Mathf.RoundToInt(Levels[CurrentLevel].Cost * RuleManager.main.GetUpgradeDiscountMod());
        if(CurrentLevel < Levels.Length && adjustedCost <= Player.main.Money)
        {
            Tower.Range = Levels[CurrentLevel].Range;
            Tower.FireRate = Levels[CurrentLevel].FireRate;
            Tower.Damage = Levels[CurrentLevel].Damage;

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
            int adjustedCost = Mathf.RoundToInt(Levels[CurrentLevel].Cost * RuleManager.main.GetUpgradeDiscountMod());
            CurrentCost = adjustedCost.ToString();
        }
    }
}
