// GameManager.cs

using System;
using UnityEngine;
using System.Collections.Generic;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance; // Singleton
    [Header("Asset References")]
    public GameObject nodePrefab;
    
    [Header("Game State")]
    public bool isGameOver = false;
    public float totalNetworkSupply = 0f;
    public float totalNetworkDemand = 0f;
    public float networkEfficiency = 1f;

    [Header("Layer Management")]
    public float layerDuplicationTime = 60f; 
    private float layerTimer = 0f;
    public float layerZSpacing = 15f; // How far apart to space layers
    public int currentLayerIndex = 0; // 0 is the present
    public List<TimeLayerState>temporalLayers = new List<TimeLayerState>();
    
    [Header("Grid Settings")]
    public int gridWidth = 10;
    public int gridHeight = 10;
    public int cellWidth = 1;
    public int cellHeight = 1;
    
    [Header("Energy Management")]
    private EnergyNetworkManager energyNetworkManager;
   

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void Start()
    {
        //test
        Vector3 testPos = SnapToGrid(new Vector3(2.3f, 3.7f, 0), 0);
        Vector3 testPos2 = SnapToGrid(new Vector3(4.6f, 1.2f, 0), 1);
        // Instantiate(nodePrefab, testPos, Quaternion.identity);
        // Instantiate(nodePrefab, testPos2, Quaternion.identity);
    }

    void Update()
    {
        if (isGameOver) return;
    }

    public void GameOver(string reason)
    {
        isGameOver = true;
        Time.timeScale = 0f; // Pause game
        Debug.LogError($"GAME OVER: {reason}");
        UIManager.Instance.ShowGameOver(reason);
    }

    private Vector3 SnapToGrid(Vector3 position, int layerNum)
    {
        int x = Mathf.RoundToInt(position.x / cellWidth) * cellWidth;
        int y = Mathf.RoundToInt(position.y / cellHeight) * cellHeight;
        float z = layerNum * layerZSpacing;
        return new Vector3(x, y, z);
    }
}