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
    public string TowerName = "Tower";

    [Header("Targeting Mode")]
    public bool First = true;
    public bool Last = false;
    public bool Strongest = false;

    [Header("Effects")]
    [SerializeField] GameObject FireEffect;

    [NonSerialized]
    public GameObject Target;
    private float CoolDown = 0f;

    void Update()
    {
        if(Target)
        {
            if(CoolDown >= FireRate)
            {
                transform.right = Target.transform.position - transform.position;
                Target.GetComponent<Enemy>().TakeDamage(Damage);
                CoolDown = 0f;
                StartCoroutine(FireEffectFunction());
            }
            else
            {
                CoolDown += 1f * Time.deltaTime;
            }
        }
    }

    IEnumerator FireEffectFunction()
    {
        FireEffect.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        FireEffect.SetActive(false);
    }
    
}
