using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Card" )]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;
    
}