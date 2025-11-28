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
    public GameObject conduitPrefab;

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
    private EnergyNetworkManager energyNetworkManager = new EnergyNetworkManager();


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
    RaycastHit[] hits = cameraController.RaycastAll();
    if (hits.Length == 0) return;

    // Sort hits so the first one is the closest
    System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

    // Take closest hit
    RaycastHit closest = hits[0];

    CoordinatePlane frame = closest.transform.GetComponentInParent<CoordinatePlane>();
    if (frame == null) return;

    Vector3 hitPoint = closest.point;
    Vector3 spawnPos = frame.WorldToLocal(hitPoint);
    frame.PlaceNode(nodeType, spawnPos);
}


    public void SpawnConduit(Node a, Node b)
    {
        if (a == null || b == null || a == b) return;

        // Avoid duplicate conduits between the same nodes
        foreach (var c in energyNetworkManager.presentConduits)
        {
            if ((c.nodeA == a && c.nodeB == b) || (c.nodeA == b && c.nodeB == a))
                return; // Already connected
        }

        GameObject conduitObj = Instantiate(conduitPrefab, Vector3.zero, Quaternion.identity);
        Conduit conduit = conduitObj.GetComponent<Conduit>();
        conduit.Initialize(a, b);
        energyNetworkManager.AddConduit(conduit);
    }


    public void GameOver(string reason)
    {
        isGameOver = true;
        Time.timeScale = 0f; // Pause game
        Debug.LogError($"GAME OVER: {reason}");
        UIManager.Instance.ShowGameOver(reason);
    }

   
}