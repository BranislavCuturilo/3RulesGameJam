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

    }

    [SerializeField] private Level[] Levels = new Level[3];
    [NonSerialized] public int CurrentLevel = 0;

    private Tower Tower;
    [SerializeField] private TowerRange TowerRange;

    void Awake()
    {
        Tower = GetComponent<Tower>();

    }

    public void Upgrade()
    {
        if(CurrentLevel < Levels.Length)
        {
            Tower.Range = Levels[CurrentLevel].Range;
            Tower.FireRate = Levels[CurrentLevel].FireRate;
            Tower.Damage = Levels[CurrentLevel].Damage;
            
            TowerRange.UpdateRange();

            CurrentLevel++;

            Debug.Log("Tower upgraded to level " + CurrentLevel);
        }
    }
}
