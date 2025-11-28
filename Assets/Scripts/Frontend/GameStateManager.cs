// GameManager.cs

using System;
using UnityEngine;
using System.Collections.Generic;
using Lukas.Simulation.Energy;
using NodeBase;

public class GameStateManager : MonoBehaviour, Interfaces.IFrontend
{
    public static GameStateManager Instance; // Singleton
    public CameraController cameraController;
    
    [Header("Asset References")]
    public GameObject nodePrefab;
    public GameObject generatorPrefab;
    public GameObject conduitPrefab;

    [Header("Game State")] public bool isGameOver = false;


    [Header("Layer Management")] public float layerDuplicationTime = 60f;
    private float layerTimer = 0f;
    public float layerZSpacing = 15f; // How far apart to space layers
    //public List<TimeLayerState> temporalLayers = new List<TimeLayerState>(); // were gonan get this from backend couse CBA
    
    

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
        RaycastHit[] hits = cameraController.RaycastAll(); //maybe replace with single ray with custom layer?
        foreach (var hit in hits)
        {
            CoordinatePlane frame = hit.transform.GetComponentInParent<CoordinatePlane>();
            Vector3 hitPoint = hit.point;
            if (frame != null)
            {
                Vector3 spawnPos = frame.WorldToLocal(hitPoint);
                if(frame.PlaceNode(nodeType, spawnPos, out GameObject newNode));
                {
                    //ConduitManager.Instance.RegisterNodeEvents(newNode.GetComponent<Node>());
                }
                break;
            }
        }
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


    
    
    
    
    
    
    
    
    
    
    public void UpdateEnergyPackets(List<EnergyPacket> energyPackets)
    {
        throw new NotImplementedException();
    }

    public void DeleteEnergyPackets(List<EnergyPacket> energyPackets)
    {
        throw new NotImplementedException();
    }
    public bool PlaceNodeVisual(AbstractNodeInstance node, int layerNum, Vector2 planePos)
    {
        throw new NotImplementedException();
    }

    public bool AddTimeSlice(int sliceNum)
    {
        throw new NotImplementedException();
    }
}