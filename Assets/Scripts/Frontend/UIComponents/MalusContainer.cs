using UnityEngine;
using UnityEngine.UI;
using Util;

public class MalusContainer : MonoBehaviour
{
    
    private float[] malusConfig;
    [SerializeField] private Malus[] malusIcons;
    [SerializeField] private Image buildupBar; 
   
    private void Start()
    {
        
        malusConfig = BalanceProvider.Balance.malusThresholds;
       
    }

    private void EvaluateIcons(float stability)
    {
        for (int i = 0; i < malusIcons.Length; i++)
        {
            bool active = stability <= malusConfig[i];
            malusIcons[i].ToggleMalus(active);
        }
    }

    public void EvaluateBuildupBar(float stability)
    {
        EvaluateIcons(stability);
        float[] t = malusConfig;

        for (int i = 0; i < t.Length; i++)
        {
            if (stability > t[i])
            {
                float upper = (i == 0) ? 1f : t[i - 1];
                float lower = t[i];

                float fill = Mathf.InverseLerp(upper, lower, stability);
                buildupBar.fillAmount = 1-fill;
                return;
            }
        }

        // Below lowest malus
        buildupBar.fillAmount = 0f;
    }
}