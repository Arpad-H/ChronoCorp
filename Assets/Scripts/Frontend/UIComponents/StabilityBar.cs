using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StabilityBar : MonoBehaviour
{
    public Image stabilityBar;
    public TextMeshProUGUI stabilityText;
    public MalusContainer malusContainer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpdateStabilityBar(float stabilityPercent)
    {
        if (stabilityBar)
        {
            stabilityBar.fillAmount = stabilityPercent;
            stabilityText.text = (int)(stabilityPercent*100) + "%";
        }
        if (malusContainer)
        {
            malusContainer.EvaluateMaluses(stabilityPercent);
        }
    }
}
