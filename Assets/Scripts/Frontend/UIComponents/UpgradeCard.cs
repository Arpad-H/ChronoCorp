using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCard : MonoBehaviour
{
    [Header("UI")]
    public Image icon;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Button button;
    
    UpgradeCardData _cardData;
   Action<UpgradeCardData> onSelected;

    public void Init(UpgradeCardData upgradeCard, System.Action<UpgradeCardData> onSelectedCallback)
    {
        _cardData = upgradeCard;
        onSelected = onSelectedCallback;

        icon.sprite = _cardData.icon;
        titleText.text = _cardData.upgradeName;
        descriptionText.text = _cardData.description;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(Select);
    }

    void Select()
    {
        onSelected?.Invoke(_cardData);
    }
}
