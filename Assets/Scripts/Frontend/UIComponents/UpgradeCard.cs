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
    
    UpgradeData data;
    System.Action<UpgradeData> onSelected;

    public void Init(UpgradeData upgrade, System.Action<UpgradeData> onSelectedCallback)
    {
        data = upgrade;
        onSelected = onSelectedCallback;

        icon.sprite = data.icon;
        titleText.text = data.upgradeName;
        descriptionText.text = data.description;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(Select);
    }

    void Select()
    {
        onSelected?.Invoke(data);
    }
}
