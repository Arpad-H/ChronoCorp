// UIManager.cs

using System;
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
    
    private UpgradeChoiceMenu upgradeChoiceMenuComponent;
    public GameObject cardChoiceMenu;
    
 
    

    void Awake()
    {
        Instance = this;
        upgradeChoiceMenuComponent = cardChoiceMenu.GetComponent<UpgradeChoiceMenu>();
    }

    private void Start()
    {
        
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
    public void ShowUpgradeChoiceMenu(List<UpgradeData> availableUpgrades)
    {
        cardChoiceMenu.SetActive(true);
        upgradeChoiceMenuComponent.ShowChoices(availableUpgrades);
    }
}