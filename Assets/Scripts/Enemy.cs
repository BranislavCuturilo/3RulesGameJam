using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float movespeed = 2f;
    
    private Rigidbody2D rb;

    private Transform CheckPoint;

    private int index = 0;

    void Start()
    {
        CheckPoint = EnemyManager.main.CheckPoints[index];
    }
    void Update()
    {
        CheckPoint = EnemyManager.main.CheckPoints[index];

        if (Vector2.Distance(CheckPoint.position, transform.position) < 0.1f)
        {
            index++;
            if (index >= EnemyManager.main.CheckPoints.Length)
            {
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

    
}
