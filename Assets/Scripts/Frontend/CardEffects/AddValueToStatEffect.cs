using System;
using Frontend;
using UnityEngine;
using Util;

[Serializable]
public class AddValueToStatEffect : UpgradeEffect
{
    public StatType stat;
    public float amount;

    public override void Apply(GameBalance balance)
    {
        balance.Add(stat, amount);
    }
}