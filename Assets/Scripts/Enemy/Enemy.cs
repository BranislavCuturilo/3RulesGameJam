using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
    public int Health = 50;
    [SerializeField] public float movespeed = 2f;
    [SerializeField] private int EnemyMoneyValue = 10;
    
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
                Player.main.ReceiveDamage(Health);
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
            Player.main.Money += Mathf.CeilToInt(EnemyMoneyValue * RuleManager.main.GetEconomyMoneyMod());
            Destroy(gameObject);
        }
    }
}
