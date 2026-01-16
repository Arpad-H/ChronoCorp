using System;
using UnityEngine;
using Util;

namespace Frontend
{
    [Serializable]
    public abstract class UpgradeEffect
    {
        public abstract void Apply(GameBalance balance);
    }
  
    public static class CardEffectEvaluator
    {
        public static void ApplyEffect(UpgradeCardData upgradeCard)
        {
            foreach (var effect in upgradeCard.effects)
            {
                effect.Apply(BalanceProvider.Balance);
            }
        }

    }
}