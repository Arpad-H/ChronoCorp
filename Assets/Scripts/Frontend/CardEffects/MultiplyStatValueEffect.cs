using Frontend;
using UnityEngine;
using Util;

[CreateAssetMenu(menuName = "Upgrade/Multiply Stat")]
public class MultiplyStatValueEffect : UpgradeEffect
{
    public StatType stat;
    public float factor;
        
    public override void Apply(GameBalance balance)
    {
        balance.Multiply(stat, factor);
    }
}