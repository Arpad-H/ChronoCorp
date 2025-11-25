// GameManager.cs

using System;
using UnityEngine;
using System.Collections.Generic;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance; // Singleton
    [Header("Asset References")] public GameObject nodePrefab;

    [Header("Game State")] public bool isGameOver = false;
    public float totalNetworkSupply = 0f;
    public float totalNetworkDemand = 0f;
    public float networkEfficiency = 1f;

    [Header("Layer Management")] public float layerDuplicationTime = 60f;
    private float layerTimer = 0f;
    public float layerZSpacing = 15f; // How far apart to space layers
    public int currentLayerIndex = 0; // 0 is the present
    public List<TimeLayerState> temporalLayers = new List<TimeLayerState>();
    


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
    }

    void Update()
    {

        
    }
    public void SpawnNode(CoordinatePlane coordPlane,Vector2 planePosition)
    {
        coordPlane.PlaceNode(nodePrefab, planePosition);
    }

    public void GameOver(string reason)
    {
        isGameOver = true;
        Time.timeScale = 0f; // Pause game
        Debug.LogError($"GAME OVER: {reason}");
        UIManager.Instance.ShowGameOver(reason);
    }

   
}