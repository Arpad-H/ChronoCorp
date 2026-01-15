using System.Collections.Generic;
using UnityEngine;

public class UpgradeChoiceMenu : MonoBehaviour
{
    public UpgradeCard cardPrefab;
    public Transform cardParent;

    public void ShowChoices(List<UpgradeData> availableUpgrades)
    {
        Clear();

        for (int i = 0; i < 3; i++)
        {
            //TODO decide if upgrades are random or sequential
            //   UpgradeData upgrade = availableUpgrades[Random.Range(0, availableUpgrades.Count)];
            UpgradeData upgrade = availableUpgrades[i];

            UpgradeCard card = Instantiate(cardPrefab, cardParent);
            card.Init(upgrade, OnUpgradeSelected);
        }
    }

    void OnUpgradeSelected(UpgradeData upgrade)
    {
        
        GameFrontendManager.Instance.UpgradeCardSelected(upgrade);
        gameObject.SetActive(false);
      
    }

    void Clear()
    {
        foreach (Transform child in cardParent)
            Destroy(child.gameObject);
    }
}