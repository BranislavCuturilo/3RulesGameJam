using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager main;
    public Transform spawnPoint;
    public Transform[] CheckPoints;

    [SerializeField] private GameObject Enemy;
    [SerializeField] private GameObject FastEnemy;
    [SerializeField] private GameObject TankEnemy;

    [SerializeField] private int Wave = 1;
    [SerializeField] private int CountEnemy = 6;
    [SerializeField] private float CountEnemyRate=0.2f;
    [SerializeField] private float SpawnDelayMax = 1f;
    [SerializeField] private float SpawnDelayMin = 0.75f;

    [SerializeField] private float EnemyRate = 0.5f;
    [SerializeField] private float FastEnemyRate = 0.4f;
    [SerializeField] private float TankEnemyRate = 0.2f;

    [SerializeField] private GameObject WavePanel;
    
    [Header("Wave Timing")]
    [SerializeField] private float preWaveDelaySeconds = 5f; 
    
    private bool WaveOver = false;
    private List<GameObject> WaveSet = new List<GameObject>();
    private int EnemyLeft;
    private bool WaveDone = false;
    private int EnemyCount;
    private int FastEnemyCount;
    private int TankEnemyCount;
    
    void Awake()
    {
        main = this;
    }
    private void SetWave()
    {
        EnemyCount = Mathf.RoundToInt(CountEnemy * (EnemyRate + TankEnemyRate));
        FastEnemyCount = Mathf.RoundToInt(CountEnemy * FastEnemyRate);
        TankEnemyCount = 0;

        if(Wave % 5 == 0)
        {
        EnemyCount = Mathf.RoundToInt(CountEnemy * EnemyRate);
        TankEnemyCount = Mathf.RoundToInt(CountEnemy * TankEnemyRate);
        }

        EnemyLeft = EnemyCount + FastEnemyCount + TankEnemyCount;
        EnemyCount = EnemyLeft;

        float qtyMul = RuleManager.main != null ? RuleManager.main.GetEnemyQuantityMod() : 1f;
        int baseTotal = Mathf.Max(0, EnemyCount); 
        int targetTotal = Mathf.Max(0, Mathf.RoundToInt(baseTotal * qtyMul));

        int baseEnemy = Mathf.Max(0, EnemyCount - FastEnemyCount - TankEnemyCount);
        int baseFast = Mathf.Max(0, FastEnemyCount);
        int baseTank = Mathf.Max(0, TankEnemyCount);
        int baseSum = Mathf.Max(1, baseEnemy + baseFast + baseTank);

        int scaledEnemy = Mathf.RoundToInt((float)baseEnemy / baseSum * targetTotal);
        int scaledFast = Mathf.RoundToInt((float)baseFast / baseSum * targetTotal);
        int scaledTank = Mathf.Max(0, targetTotal - scaledEnemy - scaledFast);

        WaveSet = new List<GameObject>(targetTotal);
        for (int i = 0; i < scaledEnemy; i++) WaveSet.Add(Enemy);
        for (int i = 0; i < scaledFast; i++) WaveSet.Add(FastEnemy);
        for (int i = 0; i < scaledTank; i++) WaveSet.Add(TankEnemy);
        WaveSet = Shuffle(WaveSet);
        
        int fixedOverride = RuleManager.main != null ? RuleManager.main.GetFixedEnemyCountOverride() : -1;
        if (fixedOverride >= 0)
        {
            if (fixedOverride < WaveSet.Count)
            {
                WaveSet = WaveSet.GetRange(0, fixedOverride);
            }
            else if (fixedOverride > WaveSet.Count)
            {
                int toAdd = fixedOverride - WaveSet.Count;
                for (int i = 0; i < toAdd; i++) WaveSet.Add(Enemy);
            }
        }
        

        StartCoroutine(Spawn());

    
    }

    public List<GameObject> Shuffle(List<GameObject> WaveSet)
    {
        List<GameObject> Result = new List<GameObject>();
        List<GameObject> Temp = new List<GameObject>();
        
        Temp.AddRange(WaveSet);
        for(int i =0; i<WaveSet.Count; i++)
        {
            int index = Random.Range(0, Temp.Count -1);
            Result.Add(Temp[index]);
            Temp.RemoveAt(index);
        }
        return Result;
    }

    IEnumerator Spawn()
    {
        int waveIndex = Wave; 

        for(int i = 0; i<WaveSet.Count; i++)
        {
            GameObject EnemyObj = Instantiate(WaveSet[i], spawnPoint.position, Quaternion.identity);
            Enemy Enemy = EnemyObj.GetComponent<Enemy>();
            float speedMul = RuleManager.main != null ? RuleManager.main.GetEnemySpeedMod() : 1f;
            Enemy.movespeed = Enemy.baseMoveSpeed * speedMul;
            EnemyStatusEffects status = EnemyObj.GetComponent<EnemyStatusEffects>();
            if (status != null) { status.SetBaseSpeed(Enemy.movespeed); }
            
            int fixedHP = RuleManager.main != null ? RuleManager.main.GetFixedEnemyHealthOverride() : -1;
            
            var rm = RuleManager.main;
            float ruleHpMul = rm != null ? rm.GetEnemyHPMod() : 1f;
            float waveHpMul = 1f + 0.05f * (waveIndex - 1);
            if (fixedHP >= 1)
            {
                Enemy.Health = fixedHP;
            }
            else
            {
                Enemy.Health = Mathf.RoundToInt(Enemy.Health * ruleHpMul * waveHpMul);
            }

            Enemy.SetEffectiveMoneyValue(-1);

            float dMin, dMax;
            if (RuleManager.main != null && RuleManager.main.TryGetSpawnDelayOverride(out dMin, out dMax))
            {
                yield return new WaitForSeconds(Random.Range(dMin, dMax));
            }
            else
            {
                yield return new WaitForSeconds(Random.Range(SpawnDelayMin, SpawnDelayMax));
            }
        }
        WaveDone = true;
    }
   
   void Start()
   {
        StartCoroutine(BeginWaveAfterDelay());
   }
   void Update()
   {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if(!WaveOver && WaveDone && enemies.Length == 0 && !Player.main.IsGameOver)
        {
            int fixedWave = RuleManager.main != null ? RuleManager.main.GetFixedWaveCompleteMoney() : -1;
            if (fixedWave >= 0)
            {
                Player.main.Money += fixedWave;
            }
            else
            {
                int baseBonus = 30 * Wave;
                Player.main.Money += Mathf.CeilToInt(baseBonus * RuleManager.main.GetEconomyBonusMod());
            }
            WaveOver = true;
            RuleManager.main.ResetModifiers();
            RuleManager.main.ShowRuleOptions();
        }
   }
    public void NextWave()
   {
        
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if(WaveDone && enemies.Length == 0)
        {
            Wave++;
            WaveDone = false;
            WaveOver = false;
            CountEnemy += Mathf.RoundToInt(CountEnemy * CountEnemyRate); 
            StartCoroutine(BeginWaveAfterDelay());
        }
   }

   private IEnumerator BeginWaveAfterDelay()
   {
        yield return new WaitForSeconds(preWaveDelaySeconds);
        SetWave();
   }
}
