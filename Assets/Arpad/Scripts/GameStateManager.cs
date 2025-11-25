// GameManager.cs

using System;
using UnityEngine;
using System.Collections.Generic;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance; // Singleton
    public CameraController cameraController;
    
    [Header("Asset References")]
    public GameObject nodePrefab;
    public GameObject generatorPrefab;

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
        if (cameraController == null) cameraController = FindObjectOfType<CameraController>();
        InputManager.Instance.OnButtonN += () => SpawnOnHoveredFrame(nodePrefab);
        InputManager.Instance.OnButtonG += () => SpawnOnHoveredFrame(generatorPrefab);
    }

    void Update()
    {

        
    }
    private void SpawnOnHoveredFrame(GameObject nodeType)
    {
        RaycastHit[] hits = cameraController.RaycastAll(); //maybe replace with single ray with custom layer?
        foreach (var hit in hits)
        {
            CoordinatePlane frame = hit.transform.GetComponentInParent<CoordinatePlane>();
            Vector3 hitPoint = hit.point;
            if (frame != null)
            {
                Vector3 spawnPos = frame.WorldToLocal(hitPoint);
                frame.PlaceNode(nodeType, spawnPos);
                break;
            }
        }
    }

    public void GameOver(string reason)
    {
        isGameOver = true;
        Time.timeScale = 0f; // Pause game
        Debug.LogError($"GAME OVER: {reason}");
        UIManager.Instance.ShowGameOver(reason);
    }

   
}