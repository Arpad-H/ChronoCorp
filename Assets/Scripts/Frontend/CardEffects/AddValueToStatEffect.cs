using Frontend;
using UnityEngine;
using Util;

[CreateAssetMenu(menuName = "Upgrade/Add Stat")]
public class AddValueToStatEffect : UpgradeEffect
{
    public StatType stat;
    public float amount;

    public override void Apply(GameBalance balance)
    {
        balance.Add(stat, amount);
    }
}