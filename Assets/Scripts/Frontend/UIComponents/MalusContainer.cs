using UnityEngine;
using UnityEngine.UI;
using Util;

public class MalusContainer : MonoBehaviour
{
    
    private float[] malusConfig;
    [SerializeField] private Malus[] malusIcons;
    [SerializeField] private Image buildupBar; 
    private float lastStability = 1f;
    private int lastIntensity = 0;
   
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
    

    private void EvaluateSound(float stability)
    {
        int currentIntensity = GetIntensity(stability);

        if (currentIntensity > lastIntensity)
        {
            // Stability dropped → enable new, stronger layer
            AudioManager.Instance.ToggleAdaptiveLayer(currentIntensity, true);
        }
        else if (currentIntensity < lastIntensity)
        {
            // Stability rose → disable old, stronger layer
            AudioManager.Instance.ToggleAdaptiveLayer(lastIntensity, false);
        }

        lastIntensity = currentIntensity;
    }

    private int GetIntensity(float stability)
    {
        if (stability < malusConfig[2])
            return 4;
        if (stability < malusConfig[1])
            return 3;
        if (stability < malusConfig[0])
            return 2;
        if (stability < 1f)
            return 1;

        return 0;
    }

    

    public void EvaluateBuildupBar(float stability)
    {
        EvaluateIcons(stability);
        EvaluateSound(stability);
        float[] t = malusConfig;

        for (int i = 0; i < t.Length; i++)
        {
            if (stability > t[i])
            {
                float upper = (i == 0) ? 1f : t[i - 1];
                float lower = t[i];

                float fill = Mathf.InverseLerp(upper, lower, stability);
                buildupBar.fillAmount = 1-fill;
                lastStability = stability;
                return;
            }
        }

        // Below lowest malus
        buildupBar.fillAmount = 0f;
        lastStability = stability;
    }
}