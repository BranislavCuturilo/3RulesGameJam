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
            Tower.Target = Targets[0];
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
