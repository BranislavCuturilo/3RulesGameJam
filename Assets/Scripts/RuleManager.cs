using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class RuleManager : MonoBehaviour
{
    public static RuleManager main;

    // UI elementi (dodaj ih u Inspectoru)
    [SerializeField] private GameObject RulePanel; // Novi panel u sredini ekrana
    [SerializeField] private Button[] OptionButtons; // 3 dugmeta za opcije
    [SerializeField] private TextMeshProUGUI[] OptionTexts; // Tekstovi za svaku opciju

    // Modifikatori (primjeri, proširi listu)
    private string[] enemyRules = { "Protivnici brži za 30%", "Protivnici imaju +20% HP", "Protivnici daju -10% novca" };
    private string[] towerRules = { "Tornjevi imaju -20% fire rate", "Tornjevi imaju +10% damage", "Tornjevi imaju manji domet za 15%" };
    private string[] economyRules = { "Sav zaradjeni novac +50%", "Bonus nakon vala x2", "Cijena nadogradnji -20%" };

    // Privremeni modifikatori (primjenjuju se na sljedeći val)
    public float enemySpeedMod = 1f; // 1f = normalno
    public float enemyHPMod = 1f;
    public float towerFireRateMod = 1f;
    public float towerDamageMod = 1f;
    public float economyMoneyMod = 1f;
    public float economyBonusMod = 1f;

    void Awake() { main = this; }

    // Pozovi ovo nakon vala (iz EnemyManager)
    public void ShowRuleOptions()
    {
        RulePanel.SetActive(true);
        for (int i = 0; i < 3; i++)
        {
            string ruleSet = GenerateRuleSet();
            OptionTexts[i].text = ruleSet;
            int index = i; // Za lambda
            OptionButtons[i].onClick.RemoveAllListeners();
            OptionButtons[i].onClick.AddListener(() => SelectOption(index));
        }
    }

    private string GenerateRuleSet()
    {
        string enemy = enemyRules[Random.Range(0, enemyRules.Length)];
        string tower = towerRules[Random.Range(0, towerRules.Length)];
        string economy = economyRules[Random.Range(0, economyRules.Length)];
        return $"{enemy}\n{tower}\n{economy}";
    }

    private void SelectOption(int index)
    {
        // Parsiraj i postavi modifikatore na osnovu teksta (proširi sa if-ovima)
        string selected = OptionTexts[index].text;
        // Primjer parsiranja (dodaj više)
        if (selected.Contains("brži za 30%")) enemySpeedMod = 1.3f;
        if (selected.Contains("+20% HP")) enemyHPMod = 1.2f;
        if (selected.Contains("-20% fire rate")) towerFireRateMod = 0.8f;
        if (selected.Contains("+10% damage")) towerDamageMod = 1.1f;
        if (selected.Contains("+50%")) economyMoneyMod = 1.5f;
        if (selected.Contains("x2")) economyBonusMod = 2f;

        RulePanel.SetActive(false);
        ApplyModifiers(); // Primijeni odmah
        EnemyManager.main.NextWave(); // Pokreni sljedeći val
    }

    private void ApplyModifiers()
    {
        // Primijeni na tornjeve (prolazi kroz sve tornjeve)
        GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower"); // Pretpostavi tag "Tower" na tornjevima
        foreach (var t in towers)
        {
            Tower tower = t.GetComponent<Tower>();
            tower.FireRate *= towerFireRateMod;
            tower.Damage = Mathf.RoundToInt(tower.Damage * towerDamageMod);
            // Dodaj za range ako treba
        }

        // Ekonomija: Modifikuj u Player ili Enemy (vidi korak 3 i 4)
    }

    // Pozovi ovo nakon sljedećeg vala da resetuješ
    public void ResetModifiers()
    {
        enemySpeedMod = 1f;
        enemyHPMod = 1f;
        towerFireRateMod = 1f;
        towerDamageMod = 1f;
        economyMoneyMod = 1f;
        economyBonusMod = 1f;

        // Resetuj tornjeve na originalne vrijednosti (čuvaj originale ako treba)
        GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");
        foreach (var t in towers)
        {
            Tower tower = t.GetComponent<Tower>();
            // Pretpostavi da imaš originalne vrijednosti sačuvane (dodaj polja u Tower.cs)
        }
    }
}