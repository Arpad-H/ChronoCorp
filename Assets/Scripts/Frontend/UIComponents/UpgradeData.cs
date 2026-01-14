using System.Collections;
using System.Collections.Generic;
using Frontend;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Card" )]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;
    [Expandable]
    public List<UpgradeEffect> effects;

   
}