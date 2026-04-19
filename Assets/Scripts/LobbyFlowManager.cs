using UnityEngine;
using TMPro;

public class LobbyFlowManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject titlePanel;
    public GameObject rulesPanel;
    public GameObject rolePanel;

    [Header("Rules Content")]
    public TextMeshProUGUI rulesText;
    [TextArea(3, 10)]
    public string[] rulesPages;
    private int currentRulePage = 0;

    void Start() => ShowTitle();

    public void ShowTitle()
    {
        titlePanel.SetActive(true);
        rulesPanel.SetActive(false);
        rolePanel.SetActive(false);
    }

    public void StartRules()
    {
        titlePanel.SetActive(false);
        rulesPanel.SetActive(true);
        currentRulePage = 0;
        UpdateRulesDisplay();
    }

    public void NextRule()
    {
        if (currentRulePage < rulesPages.Length - 1)
        {
            currentRulePage++;
            UpdateRulesDisplay();
        }
        else
        {
            ShowRoleSelection();
        }
    }

    public void PreviousRule()
    {
        if (currentRulePage > 0)
        {
            currentRulePage--;
            UpdateRulesDisplay();
        }
    }

    void UpdateRulesDisplay()
    {
        rulesText.text = rulesPages[currentRulePage];
    }

    void ShowRoleSelection()
    {
        rulesPanel.SetActive(false);
        rolePanel.SetActive(true);
    }
}