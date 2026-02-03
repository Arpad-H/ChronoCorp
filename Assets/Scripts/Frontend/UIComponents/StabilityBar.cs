using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StabilityBar : MonoBehaviour
{
    public Image stabilityBar;
    public TextMeshProUGUI stabilityText;
    public MalusContainer malusContainer;
    
    [Header("Smoothing")]
    [SerializeField] private float fillSpeed = 6f; 

    private float target;    
    private float current;   
    
    void Start()
    {
        current = stabilityBar ? stabilityBar.fillAmount : 0f;
        target = current;
    }

    // Update is called once per frame
    void Update()
    {
        // Smooth toward target once per frame.
        current = Mathf.Lerp(current, target, 1f - Mathf.Exp(-fillSpeed * Time.unscaledDeltaTime));

        if (stabilityBar) stabilityBar.fillAmount = current;
        if (stabilityText) stabilityText.text = Mathf.RoundToInt(current * 100f) + "%";

        if (malusContainer)
            malusContainer.EvaluateBuildupBar(current); 
    }
    public void UpdateStabilityBar(float stabilityPercent)
    {
        target = Mathf.Clamp01(stabilityPercent);
        if (stabilityBar)
        {
            stabilityBar.fillAmount = stabilityPercent;
            stabilityText.text = (int)(stabilityPercent*100) + "%";
        }
        if (malusContainer)
        {
            malusContainer.EvaluateBuildupBar(stabilityPercent);
        }
    }
}
