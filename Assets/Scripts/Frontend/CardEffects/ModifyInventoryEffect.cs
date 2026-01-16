using System;
using Frontend;
using Interfaces;
using UnityEngine;
using Util;

[Serializable]
    public class ModifyInventoryEffect : UpgradeEffect
    {
        public InventoryItem item;  
        public float amount;

        public override void Apply(GameBalance balance)
        {
           GameFrontendManager.Instance.AddToInventory(item, amount);
        }
    }
