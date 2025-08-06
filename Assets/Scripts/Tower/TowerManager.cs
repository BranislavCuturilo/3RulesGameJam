using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class TowerManager : MonoBehaviour
{
    [Header("Tower Prefabs")]
    [SerializeField] private GameObject CannonTower;
    [SerializeField] private GameObject SniperTower;
    [SerializeField] private GameObject MachineGunTower;
    [SerializeField] private GameObject ShotgunTower;

    [SerializeField] private LayerMask TowerLayer;

    private GameObject SelectedTower;
    private GameObject PlacingTower;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetTower(CannonTower);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetTower(SniperTower);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetTower(MachineGunTower);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetTower(ShotgunTower);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
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
            }
            else
            {
                SelectedTower = null;
            }
        }
        if(Input.GetKeyDown(KeyCode.U) && SelectedTower)
        {
            SelectedTower.GetComponent<TowerUpgrade>().Upgrade();
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
    private void SetTower(GameObject Tower)
    {
        ClearSelectedTower();
        PlacingTower = Instantiate(Tower);
    }
}
