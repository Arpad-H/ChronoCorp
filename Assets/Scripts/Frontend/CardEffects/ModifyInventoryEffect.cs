using Frontend;
using UnityEngine;
using Util;

[CreateAssetMenu(menuName = "Upgrade/ModifyInventoryEffect")]
    public class ModifyInventoryEffect : UpgradeEffect
    {
        public StatType stat; //TODO 
        public float amount;

        public override void Apply(GameBalance balance)
        {
            balance.Add(stat, amount);
        }
    }
