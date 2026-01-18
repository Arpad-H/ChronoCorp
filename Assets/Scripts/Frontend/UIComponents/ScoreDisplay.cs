using TMPro;
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI TargetText;
    
    const int DIGITS = 5;

    public void SetTargetScore(int target)
    {
        TargetText.text = "/" + target.ToString($"D{DIGITS}");
    }

    public void SetCurrentScore(int score)
    {
        ScoreText.text = score.ToString($"D{DIGITS}");
    }


}
