using UnityEngine;
using Util;

namespace Frontend
{
    public abstract class UpgradeEffect : ScriptableObject
    {
        public abstract void Apply(GameBalance balance);
    }
  
    public static class CardEffectEvaluator
    {
        public static void ApplyEffect(UpgradeData upgrade)
        {
            foreach (var effect in upgrade.effects)
            {
                effect.Apply(BalanceProvider.Balance);
            }
        }

    }
}