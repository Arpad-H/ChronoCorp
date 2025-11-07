// UIManager.cs
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TextMeshProUGUI statusText; // Assign from Hierarchy
    public GameObject gameOverPanel; // Create a simple Panel in your Canvas
    public TextMeshProUGUI gameOverText;

    void Awake()
    {
        Instance = this;
        // Find the objects if not assigned
        if (statusText == null) statusText = GameObject.Find("StatusText").GetComponent<TextMeshProUGUI>();
        // (Find GameOver panel similarly)
        // gameOverPanel.SetActive(false); 
    }

    public void UpdateStatus(float supply, float demand, int pastLayers, float timer)
    {
      string status = $"Supply: {supply:F0} CkW\n"; 
        status += $"Demand: {demand:F0} CkW\n";
        status += $"Efficiency: {(supply / (demand + 0.001f) * 100):F0}%\n";
        status += $"Past Layers: {pastLayers}\n";
        status += $"Next Layer In: {(60 - timer):F0}s";
        
        statusText.text = status;

        // Change color based on efficiency
        if (supply < demand)
            statusText.color = Color.red;
        else
            statusText.color = Color.green;
    }

    public void ShowGameOver(string reason)
    {
        // In a real build, you'd enable a panel
        // gameOverPanel.SetActive(true);
        // gameOverText.text = reason;
        statusText.text = $"GAME OVER\n{reason}";
        statusText.color = Color.red;
    }
}