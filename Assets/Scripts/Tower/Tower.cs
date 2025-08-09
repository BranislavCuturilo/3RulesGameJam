using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Tower : MonoBehaviour
{
    [Header("Tower Stats")]
    public float Range = 8f;
    public float FireRate = 1f;
    public int Damage = 25;
    public int Cost = 50;

    [Header("Tower Stats")]
    public float originalRange;
    public float originalFireRate;
    public int originalDamage;

    public string TowerName = "Tower";

    [Header("Targeting Mode")]
    public bool First = true;
    public bool Last = false;
    public bool Strongest = false;

    [Header("Effects & Firing")]
    [SerializeField] private GameObject defaultFireEffect;
    [SerializeField] private GameObject[] fireEffectsByLevel;
    [SerializeField] private float fireEffectBaseDuration = 0.5f;
    [Tooltip("Scale applied to base duration. 0.4 = 60% shorter than base")]
    [SerializeField] private float fireEffectDurationFactor = 0.4f;
    [SerializeField] public Transform firePoint; // mjesto spawna projektila

    [NonSerialized]
    public GameObject Target;
    private float CoolDown = 0f;
    private TowerEffects towerEffects;

    void Awake()
    {
        originalRange = Range;
        originalFireRate = FireRate;
        originalDamage = Damage;
        towerEffects = GetComponent<TowerEffects>();
    }
    void Update()
    {
        if(Target)
        {
            if(CoolDown >= FireRate)
            {
                transform.right = Target.transform.position - transform.position;
                
                // Izgradi ShotData i ispali projektil (ili direktni pogodak ako nema projektila)
                ShotData shot = TowerShotBuilder.BuildShotData(this, towerEffects);
                SpawnProjectileOrHit(shot, Target);
                
                CoolDown = 0f;
                if (defaultFireEffect != null || (fireEffectsByLevel != null && fireEffectsByLevel.Length > 0))
                {
                    StartCoroutine(FireEffectFunction());
                }
            }
            else
            {
                CoolDown += 1f * Time.deltaTime;
            }
        }
    }

    IEnumerator FireEffectFunction()
    {
        GameObject effectToUse = defaultFireEffect;
        var upgrade = GetComponent<TowerUpgrade>();
        int levelIndex = upgrade != null ? Mathf.Clamp(upgrade.CurrentLevel, 0, int.MaxValue) : 0;
        if (fireEffectsByLevel != null && fireEffectsByLevel.Length > 0)
        {
            int idx = Mathf.Clamp(levelIndex, 0, fireEffectsByLevel.Length - 1);
            if (fireEffectsByLevel[idx] != null)
            {
                effectToUse = fireEffectsByLevel[idx];
            }
        }

        if (effectToUse != null)
        {
            effectToUse.SetActive(true);
            float duration = Mathf.Max(0f, fireEffectBaseDuration * fireEffectDurationFactor); // 60% shorter if factor=0.4
            yield return new WaitForSeconds(duration);
            effectToUse.SetActive(false);
        }
    }
    
    private void SpawnProjectileOrHit(ShotData shot, GameObject target)
    {
        if (shot.projectilePrefab != null)
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            GameObject projectile = Instantiate(shot.projectilePrefab, spawnPos, Quaternion.identity);
            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript == null)
            {
                projScript = projectile.AddComponent<Projectile>();
            }
            projScript.Initialize(shot, target);
        }
        else
        {
            // Fallback: direktan pogodak bez projektila
            Enemy enemy = target != null ? target.GetComponent<Enemy>() : null;
            if (enemy != null)
            {
                enemy.TakeDamage(shot.damage);
            }
        }
    }
    
}
