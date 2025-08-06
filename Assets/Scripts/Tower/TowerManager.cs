using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using TMPro;
using UnityEngine.EventSystems;

public class TowerManager : MonoBehaviour
{
    [Header("Tower Prefabs")]
    [SerializeField] private GameObject CannonTower;
    [SerializeField] private GameObject SniperTower;
    [SerializeField] private GameObject MachineGunTower;
    [SerializeField] private GameObject ShotgunTower;

    [SerializeField] private LayerMask TowerLayer;

    [SerializeField] private GameObject Panel;
    [SerializeField] private TextMeshProUGUI TowerName;
    [SerializeField] private TextMeshProUGUI TowerLevel;
    [SerializeField] private TextMeshProUGUI UpgradeCost;
    [SerializeField] private TextMeshProUGUI TowerTargeting;

    private GameObject SelectedTower;
    private GameObject PlacingTower;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearSelectedTower();
        }
        if(PlacingTower)
        {
            if(!PlacingTower.GetComponent<TowerPlacement>().IsPlacing)
            {
                PlacingTower = null;
            }
        }
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit2D Hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero,100f, TowerLayer);
            if(Hit.collider != null)
            {
                if(SelectedTower)
                {
                    GameObject Range1 = SelectedTower.transform.GetChild(1).gameObject;

                    Range1.GetComponent<SpriteRenderer>().enabled = false;
                }
                SelectedTower = Hit.collider.gameObject;
                GameObject Range2 = SelectedTower.transform.GetChild(1).gameObject;
                Range2.GetComponent<SpriteRenderer>().enabled = true;

                Panel.SetActive(true);
                TowerName.text = SelectedTower.GetComponent<Tower>().TowerName.Replace("(Clone)", "").Trim();
                TowerLevel.text = "Tower LVL: " + SelectedTower.GetComponent<TowerUpgrade>().CurrentLevel.ToString();
                UpgradeCost.text = SelectedTower.GetComponent<TowerUpgrade>().CurrentCost;
                
                Tower Tower = SelectedTower.GetComponent<Tower>();
                if(Tower.First)
                {
                    TowerTargeting.text = "First";
                }
                else if(Tower.Last)
                {
                    TowerTargeting.text = "Last";
                }
                else if(Tower.Strongest)
                {
                    TowerTargeting.text = "Strongest";
                }    
            }
            else if(!EventSystem.current.IsPointerOverGameObject() && SelectedTower)
            {
                Panel.SetActive(false);
                GameObject Range1 = SelectedTower.transform.GetChild(1).gameObject;

                Range1.GetComponent<SpriteRenderer>().enabled = false;
                SelectedTower = null;
            }
        }
    }
    private void ClearSelectedTower()
    {
        if(PlacingTower)
        {
            Destroy(PlacingTower);
            PlacingTower = null;
        }
    }
    public void SetTower(GameObject Tower)
    {
        ClearSelectedTower();
        PlacingTower = Instantiate(Tower);
    }
    public void UpgradeSelectedTower()
    {
        if(SelectedTower)
        {
            SelectedTower.GetComponent<TowerUpgrade>().Upgrade();
        }
    }
    public void ChangeTargeting()
    {
        if(SelectedTower)
        {
            Tower Tower = SelectedTower.GetComponent<Tower>();
            if(Tower.First)
            {
                Tower.First = false;
                Tower.Last = true;
                Tower.Strongest = false;
                TowerTargeting.text = "Last";
            }
            else if(Tower.Last)
            {
                Tower.First = false;
                Tower.Last = false;
                Tower.Strongest = true;
                TowerTargeting.text = "Strongest";
            }
            else if(Tower.Strongest)
            {
                Tower.First = true;
                Tower.Last = false;
                Tower.Strongest = false;
                TowerTargeting.text = "First";
            }
        }
    }
}