using System.Collections.Generic;
using UnityEngine;

public class UpgradeChoiceMenu : MonoBehaviour
{
    public UpgradeCard cardPrefab;
    public Transform cardParent;
    public AudioSource selectedAudioSource;

    public void ShowChoices(List<UpgradeCardData> availableUpgrades)
    {
        GameFrontendManager.Instance.SetGameState(GameFrontendManager.GameState.PAUSED);
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
        selectedAudioSource.Play();
      GameFrontendManager.Instance.SetGameState(GameFrontendManager.GameState.PLAYING);
    }

    void Clear()
    {
        foreach (Transform child in cardParent)
            Destroy(child.gameObject);
    }
}