using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager main;
    public Transform spawnPoint;
    public Transform[] CheckPoints;

    [SerializeField] private GameObject Enemy;
    [SerializeField] private GameObject FastEnemy;
    [SerializeField] private GameObject TankEnemy;

    [SerializeField] private int CountEnemy = 6;

    [SerializeField] private float EnemyRate = 0.5f;
    [SerializeField] private float FastEnemyRate = 0.4f;
    [SerializeField] private float TankEnemyRate = 0.2f;

    private List<GameObject> WaveSet = new List<GameObject>();
    private int EnemyLeft;

    private int EnemyCount;
    private int FastEnemyCount;
    private int TankEnemyCount;
    
    void Awake()
    {
        main = this;
    }
    private void SetWave()
    {
        EnemyCount = Mathf.RoundToInt(CountEnemy * EnemyRate);
        FastEnemyCount = Mathf.RoundToInt(CountEnemy * FastEnemyRate);
        TankEnemyCount = Mathf.RoundToInt(CountEnemy * TankEnemyRate);

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
        
        StartCoroutine(Spawn());

    
    }

    IEnumerator Spawn()
    {
        for(int i = 0; i<WaveSet.Count; i++)
        {
            Instantiate(WaveSet[i], spawnPoint.position, Quaternion.identity);
            yield return new WaitForSeconds(0.5f);
        }
    }
   
   void Start()
   {
    SetWave();
   }
}
