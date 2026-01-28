using UnityEngine;

public class GameOverScript : MonoBehaviour
{
    int score = 0;
    private string text1 ="Chrono Corp can not tolerate failure.\n" ;
    private string text2 = "You are fired.";
    private string text3 =     "\nChrono Corp thanks you for serving\npaying customers: ";
    public Color scoreColor;
    public TMPro.TextMeshProUGUI scoreText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnReset()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    public void SetScore(int finalScore)
    {
        score = finalScore;
        string colorHex1 = ColorUtility.ToHtmlStringRGB(scoreColor);
        string colorHex2 = ColorUtility.ToHtmlStringRGB(Color.red);
        
        scoreText.text = $"{text1}<color=#{colorHex2}>{text2}</color>{text3}</color><color=#{colorHex1}>{score}</color>";
    }
}
