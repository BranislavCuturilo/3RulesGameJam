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

    void Awake()
    {
        RangeCollider.enabled = false;
    }

    void Update()
    {    
        if(IsPlacing)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = mousePosition;
        }
        if(Input.GetMouseButtonDown(1) && !IsRestricted)
        {
            RangeCollider.enabled = true;
            IsPlacing = false;
            RangeCircle.enabled = false;
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
        if(collision.gameObject.tag == "Restricted" || collision.gameObject.tag == "Tower" && IsPlacing)
        {
            IsRestricted = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Restricted" || collision.gameObject.tag == "Tower" && IsPlacing)
        {
            IsRestricted = false;
        }
    }
}
