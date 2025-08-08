using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public static Player main;

    public int Money = 1000;
    [SerializeField] private int Health = 500;
    [NonSerialized] public bool IsGameOver = false;

    [SerializeField] private TextMeshProUGUI MoneyText;
    [SerializeField] private TextMeshProUGUI HealthText;

    [SerializeField] private GameObject GameOverGUI;

    void Awake()
    {
        main = this;
    }
    void Update()
    {
        MoneyText.text = "Money: " + Money.ToString();
        HealthText.text = "HP: " + Health.ToString();
    }
    public void ReceiveDamage(int damage)
    {
        Health -= damage;
        if(Health <= 0)
        {
            IsGameOver = true;
            GameOverGUI.SetActive(true);
        }
    }
    public void Restart()
    {
        string CurrentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(CurrentSceneName);
    }
    public void Quit()
    {
        Application.Quit();
    }
}
