using System.Collections;
using System.Collections.Generic;
using Frontend;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Card" )]
public class UpgradeCardData : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;
    [SubclassSelector]
    [SerializeReference]
    public List<UpgradeEffect> effects;
}