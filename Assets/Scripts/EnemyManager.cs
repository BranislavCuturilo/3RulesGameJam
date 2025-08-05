using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager main;
    public Transform[] CheckPoints;
   
    void Awake()
    {
        main = this;
    }

   
}
