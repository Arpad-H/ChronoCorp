using System.Collections.Generic;
using UnityEngine;

public class UpgradeChoiceMenu : MonoBehaviour
{
    public UpgradeCard cardPrefab;
    public Transform cardParent;

    public void ShowChoices(List<UpgradeCardData> availableUpgrades)
    {
        Clear();

        for (int i = 0; i < 3; i++)
        {
            //TODO decide if upgrades are random or sequential
            //   UpgradeData upgrade = availableUpgrades[Random.Range(0, availableUpgrades.Count)];
            UpgradeCardData upgradeCard = availableUpgrades[i];

            UpgradeCard card = Instantiate(cardPrefab, cardParent);
            card.Init(upgradeCard, OnUpgradeSelected);
        }
    }

    void OnUpgradeSelected(UpgradeCardData upgradeCard)
    {
        
        GameFrontendManager.Instance.UpgradeCardSelected(upgradeCard);
        gameObject.SetActive(false);
      
    }

    void Clear()
    {
        foreach (Transform child in cardParent)
            Destroy(child.gameObject);
    }
}