using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    [SerializeField] private SpriteRenderer RangeCircle;
    [SerializeField] private CircleCollider2D RangeCollider;
    [SerializeField] private Color GrayColor;
    [SerializeField] private Color RedColor;
    [NonSerialized] public bool IsPlacing = true;
    private bool IsRestricted = false;
    private readonly System.Collections.Generic.HashSet<Collider2D> blockingColliders = new System.Collections.Generic.HashSet<Collider2D>();

    private Tower Tower;

    void Awake()
    {
        Tower = GetComponent<Tower>();
        RangeCollider.enabled = false;
    }

    void Update()
    {    
        if(IsPlacing)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = mousePosition;
        }
        
        IsRestricted = blockingColliders.Count > 0;
        int adjustedCost = Mathf.RoundToInt(Tower.Cost * RuleManager.main.GetTowerPlacementCostMod());
        if(Input.GetMouseButtonDown(0) && !IsRestricted && adjustedCost <= Player.main.Money)
        {
            RangeCollider.enabled = true;
            IsPlacing = false;
            RangeCircle.enabled = false;
            Player.main.Money -= adjustedCost;
            GetComponent<TowerPlacement>().enabled = false;
        }
        if(IsRestricted)
        {
            RangeCircle.color = RedColor;
        }
        else
        {
            RangeCircle.color = GrayColor;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(!IsPlacing) return;
        string tag = collision.gameObject.tag;
        if (tag == "Restricted" || tag == "Tower")
        {
            blockingColliders.Add(collision);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(!IsPlacing) return;
        string tag = collision.gameObject.tag;
        if (tag == "Restricted" || tag == "Tower")
        {
            blockingColliders.Remove(collision);
        }
    }

    void OnDisable()
    {
        blockingColliders.Clear();
        IsRestricted = false;
    }
}
