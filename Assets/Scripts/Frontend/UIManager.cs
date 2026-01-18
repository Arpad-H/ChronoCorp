// UIManager.cs

using System;
using System.Collections;
using System.Collections.Generic;
using Frontend.UIComponents;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Util;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject deleteButtonPrefab;
    private GameObject deleteNodeButton;
    public GameObject gameOverScreen;
    public ConduitVisualizer conduitVisualizer;
    
    private UpgradeChoiceMenu upgradeChoiceMenuComponent;
    public GameObject cardChoiceMenu;
    private UpgradeCardData[] allUpgrades;
    public ScoreDisplay scoreDisplay;

    void Awake()
    {
        Instance = this;
        upgradeChoiceMenuComponent = cardChoiceMenu.GetComponent<UpgradeChoiceMenu>();
        LoadAllCards();
    }

    private void Start()
    {
       InputManager.Instance.OnRightClick += CancelDeleteButton;
    }

    private void CancelDeleteButton()
    {
        if (deleteNodeButton != null)
        {
            Destroy(deleteNodeButton);
        }
    }

    public void ShowGameOver(string reason)
    {
        gameOverScreen.SetActive(true);
    }

    public DeleteButton SpawnDeleteButton(Vector3 position)
    {
        if (deleteNodeButton != null)
        {
            Destroy(deleteNodeButton);
        }
        deleteNodeButton = Instantiate(deleteButtonPrefab, position, Quaternion.identity);
        return deleteNodeButton.GetComponent<DeleteButton>();
    }
    public void ShowUpgradeChoiceMenu()
    {
        StartCoroutine(QueueUpgradeMenu());
    }
    private IEnumerator QueueUpgradeMenu()
    {
        // This loop stays active as long as it is NOT safe
        while (!SafeToShowUpgradeUI())
        {
            yield return null; // Wait for the next frame and check again
        }

        // Once the loop breaks, it's safe!
        ExecuteShowMenu();
    }
    private void ExecuteShowMenu()
    {
        List<UpgradeCardData> availableUpgrades = GetRandomUpgrades(BalanceProvider.Balance.cardsShownPerUpgradeChoice);
        cardChoiceMenu.SetActive(true);
        upgradeChoiceMenuComponent.ShowChoices(availableUpgrades);
    }
    private List<UpgradeCardData> GetRandomUpgrades(int i)
    {
        List<UpgradeCardData> selectedUpgrades = new List<UpgradeCardData>();
        List<int> usedIndices = new List<int>();

        System.Random rand = new System.Random();

        while (selectedUpgrades.Count < i && selectedUpgrades.Count < allUpgrades.Length)
        {
            int index = rand.Next(allUpgrades.Length);
            if (!usedIndices.Contains(index))
            {
                usedIndices.Add(index);
                selectedUpgrades.Add(allUpgrades[index]);
            }
        }
        return selectedUpgrades;
    }

    private void LoadAllCards()
    {
       allUpgrades = Resources.LoadAll<UpgradeCardData>("UpgradeCardData");
    }
    
    private bool SafeToShowUpgradeUI()
    {
        return !conduitVisualizer.IsDraggingConduit();
    }
    public void SetScore(int score)
    {
        scoreDisplay.SetCurrentScore(score);
    }
    public void SetTargetScore(int score)
    {
        scoreDisplay.SetTargetScore(score);
    }
    
}