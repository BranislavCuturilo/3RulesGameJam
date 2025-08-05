using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TowerRange : MonoBehaviour
{
    [SerializeField] private Tower Tower;

    private List<GameObject> Targets = new List<GameObject>();

    void Start()
    {
        UpdateRange();
    }
    void Update()
    {
        if(Targets.Count > 0)
        {
            if(Tower.First)
            {
                float MinDistance = Mathf.Infinity;
                int MaxIndex = 0;
                GameObject FirstTarget = null;
                foreach(GameObject Target in Targets)
                {
                    int index = target.GetComponent<Enemy>().index;
                    float Distance = Target.GetComponent<Enemy>().Distance
                    if(index>MaxIndex || (index==MaxIndex && Distance<MinDistance))
                    {
                        MaxIndex = index;
                        MinDistance = Distance;
                        FirstTarget = Target;
                    }
                }
                Tower.Target = FirstTarget;
            }
            else if(Tower.Last)
            {
                float MaxDistance = -Mathf.Infinity;
                int MinIndex = int.MaxValue;
                GameObject LastTarget = null;
                foreach(GameObject Target in Targets)
                {
                    int index = target.GetComponent<Enemy>().index;
                    float Distance = Target.GetComponent<Enemy>().Distance
                    if(index<MinIndex || (index==MinIndex && Distance>MaxDistance))
                    {
                        MinIndex = index;
                        MaxDistance = Distance;
                        LastTarget = Target;
                    }
                }
                Tower.Target = LastTarget;
            }
            else if(Tower.Strongest)
            {
                GameObject StrongestTarget = null;
                float MaxHealth = 0;
                foreach(GameObject Target in Targets)
                {
                    float Health = Target.GetComponent<Enemy>().Health;
                    if(Health > MaxHealth)
                    {
                        MaxHealth = Health;
                        StrongestTarget = Target;
                    }
                }
                Tower.Target = StrongestTarget;
            }
            else
            {
                Tower.Target = Targets[0];
            }
        }
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Enemy"))
        {
            Targets.Add(collision.gameObject);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Enemy"))
        {
            Targets.Remove(collision.gameObject);
        }
    }

    public void UpdateRange()
    {
        transform.localScale = new Vector3(Tower.Range, Tower.Range, Tower.Range);
    }
}
