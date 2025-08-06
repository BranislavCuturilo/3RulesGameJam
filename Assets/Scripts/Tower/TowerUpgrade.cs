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
        CurrentCost = Levels[0].Cost.ToString();
    }

    public void Upgrade()
    {
        if(CurrentLevel < Levels.Length && Levels[CurrentLevel].Cost < Player.main.Money)
        {
            Tower.Range = Levels[CurrentLevel].Range;
            Tower.FireRate = Levels[CurrentLevel].FireRate;
            Tower.Damage = Levels[CurrentLevel].Damage;

            Player.main.Money -= Levels[CurrentLevel].Cost;
            
            TowerRange.UpdateRange();

            CurrentLevel++;

            if(CurrentLevel >= Levels.Length)
            {
                CurrentCost = "MAX";
            }
            else
            {
                CurrentCost = Levels[CurrentLevel].Cost.ToString();
            }
        }
    }
}
