using UnityEngine;
using Util;

public class MalusContainer : MonoBehaviour
{
    
    private float[] malusConfig;
    [SerializeField] private Malus[] malusIcons;

   
    private void Start()
    {
        
        malusConfig = BalanceProvider.Balance.malusThresholds;
       
    }

    public void EvaluateMaluses(float stability)
    {
        int count = Mathf.Min(
            malusConfig.Length,
            malusIcons.Length
        );

        for (int i = 0; i < count; i++)
        {
            bool active = stability <= malusConfig[i];
            malusIcons[i].ToggleMalus(active);
        }
    }
}