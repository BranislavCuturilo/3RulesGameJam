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

        WaveSet = new List<GameObject>();

        for(int i = 0; i<EnemyCount; i++)
        {
            WaveSet.Add(Enemy);
        }
        for(int i = 0; i<FastEnemyCount; i++)
        {
            WaveSet.Add(FastEnemy);
        }
        for(int i = 0; i<TankEnemyCount; i++)
        {
            WaveSet.Add(TankEnemy);
        }
        WaveSet = Shuffle(WaveSet);
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
        for(int i = 0; i<WaveSet.Count; i++)
        {
            Instantiate(WaveSet[i], spawnPoint.position, Quaternion.identity);
            yield return new WaitForSeconds(Random.Range(SpawnDelayMin, SpawnDelayMax));
        }
        WaveDone = true;

    }
   
   void Start()
   {
        SetWave();
   }
   void Update()
   {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if(!WaveOver && WaveDone && enemies.Length == 0)
        {
            Player.main.Money += 50 + (Wave * 10);
            WaveOver = true;
            WavePanel.SetActive(true);
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
            SetWave();
            WavePanel.SetActive(false);
        }
   }
}
