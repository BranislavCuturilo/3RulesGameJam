using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
    public int Health = 50;
    [SerializeField] public float movespeed = 2f;
    [SerializeField] private int EnemyMoneyValue = 10;
    [SerializeField] public int LeakDamage = 1; // Koliko damage-a nanosi igraču kada dođe do kraja
    [NonSerialized] public int EffectiveMoneyValue = -1; // Postavlja se na spawnu progresijom
    
    private Rigidbody2D rb;

    private Transform CheckPoint;

    [NonSerialized] public int index = 0;
    [NonSerialized] public float Distance = 0;

    void Start()
    {
        CheckPoint = EnemyManager.main.CheckPoints[index];
    }
    void Update()
    {
        CheckPoint = EnemyManager.main.CheckPoints[index];
        Distance = Vector2.Distance(transform.position, EnemyManager.main.CheckPoints[index].position);

        if (Vector2.Distance(CheckPoint.position, transform.position) < 0.1f)
        {
            index++;
            if (index >= EnemyManager.main.CheckPoints.Length)
            {
                // Odredi koji leak damage koristiti: override iz RuleManager-a ili lokalni LeakDamage
                int ruleOverride = RuleManager.main != null ? RuleManager.main.GetEnemyLeakDamageOverride() : -1;
                int leakDmg = ruleOverride >= 0 ? ruleOverride : LeakDamage;
                Player.main.ReceiveDamage(leakDmg);
                Destroy(gameObject);
            }
            
        }
        
    }
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Vector2 direction = (CheckPoint.position - transform.position).normalized;
        transform.right = CheckPoint.position - transform.position;
        rb.linearVelocity = direction * movespeed;
    }

    public void TakeDamage(int Damage)
    {
        Health -= Damage;
        if(Health <= 0)
        {
            int moneyBase = EffectiveMoneyValue >= 0 ? EffectiveMoneyValue : EnemyMoneyValue;
            Player.main.Money += Mathf.CeilToInt(moneyBase * RuleManager.main.GetEconomyMoneyMod());
            Destroy(gameObject);
        }
    }

    // Accessors za EnemyManager
    public int GetBaseMoneyValue() { return EnemyMoneyValue; }
    public void SetEffectiveMoneyValue(int value) { EffectiveMoneyValue = value; }
}
