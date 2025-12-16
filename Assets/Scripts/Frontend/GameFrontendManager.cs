// GameManager.cs

using System;
using System.Collections.Generic;
using Backend.Simulation.World;
using Interfaces;
using NodeBase;
using UnityEditor;
using UnityEngine;

public class GameFrontendManager : MonoBehaviour, IFrontend
{
    public static GameFrontendManager Instance;
    public CameraController cameraController;

    [Header("Asset References")] public GameObject conduitPrefab;

    public CoordinatePlane layer0; //TODO temp hardcode

   
    [Header("Layer Management")] public float layerDuplicationTime = 60f;
 private Dictionary<int, CoordinatePlane> layerToCoordinatePlane = new();
 
    public float layerZSpacing = 15f; // How far apart to space layers
    private IBackend backend; // Link to backend
    private EnergyPacketVisualizer energyPacketVisualizer;

    private long fixedTickCount;
    private float layerTimer = 0f;
   


    private void Awake()
    {
        backend = new BackendImpl(this);
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (energyPacketVisualizer == null) energyPacketVisualizer = FindObjectOfType<EnergyPacketVisualizer>();
        if (cameraController == null) cameraController = FindObjectOfType<CameraController>();
        InputManager.Instance.OnButtonN += () => SpawnManuallyOnHoveredFrame(NodeDTO.RIPPLE, EnergyType.BLUE);
        InputManager.Instance.OnButtonG += () => SpawnManuallyOnHoveredFrame(NodeDTO.GENERATOR, EnergyType.BLUE);
        InputManager.Instance.OnButton1 += () => SpawnManuallyOnHoveredFrame(NodeDTO.RIPPLE, EnergyType.GREEN);
        InputManager.Instance.OnButton2 += () => SpawnManuallyOnHoveredFrame(NodeDTO.GENERATOR, EnergyType.GREEN);
        InputManager.Instance.OnButton3 += () => SpawnManuallyOnHoveredFrame(NodeDTO.RIPPLE, EnergyType.RED);
        InputManager.Instance.OnButton4 += () => SpawnManuallyOnHoveredFrame(NodeDTO.GENERATOR, EnergyType.RED);
        InputManager.Instance.OnButton5 += () => SpawnManuallyOnHoveredFrame(NodeDTO.RIPPLE, EnergyType.YELLOW);
        InputManager.Instance.OnButton6 += () => SpawnManuallyOnHoveredFrame(NodeDTO.GENERATOR, EnergyType.YELLOW);
        InputManager.Instance.OnButtonX += () => DeleteNodeManually();
        
        //get all existing layers in scene
        var existingLayers = FindObjectsOfType<CoordinatePlane>();
        foreach (var layer in existingLayers) layerToCoordinatePlane[layer.layerNum] = layer;
    }

    private void Update()
    {
    }

    private void FixedUpdate()
    {
        Debug.Log(fixedTickCount);
        backend.tick(fixedTickCount, this);
        fixedTickCount++;
    }


    public void GameOver(string reason)
    {
        Time.timeScale = 0f; // Pause game
        Debug.LogError($"GAME OVER: {reason}");
        UIManager.Instance.ShowGameOver(reason);
    }

    public bool PlaceNodeVisual(GUID id,NodeDTO nodeDto, int layerNum, Vector2 cellPos, EnergyType energyType)
    {
        var frame = GetCoordinatePlane(layerNum);
        frame.PlaceNode(nodeDto, cellPos, out var newNode, energyType);
        newNode.GetComponent<NodeVisual>().backendID = id;
        return true;
    }


    public void SpawnEnergyPacket(GUID guid, EnergyType energyType)
    {
        energyPacketVisualizer.SpawnEnergyPacket(guid, backend, energyType);
    }

    public void DeleteEnergyPacket(GUID guid)
    {
        energyPacketVisualizer.DeleteEnergyPacket(guid);
    }

    public void OnStabilityBarUpdate(int minValue, int maxValue, int currentValue)
    {
    }

    public void OnActivateStabilityMalus(StabilityMalusType stabilityMalusType)
    {
    }

    public void OnDeactivateStabilityMalus(StabilityMalusType stabilityMalusType)
    {
    }

    public bool AddTimeSlice(int sliceNum)
    {
        var newLayer = Instantiate(layer0, new Vector3(0, 0, sliceNum * layerZSpacing), Quaternion.identity);
        newLayer.layerNum = sliceNum;
        //TODO initialize layer properly
        layerToCoordinatePlane[sliceNum] = newLayer;
        return true;
    }

    //when a button is pressed to spawn a node
    private bool SpawnManuallyOnHoveredFrame(NodeDTO nodeType, EnergyType energyType)
    {
        RaycastHit rh;
        cameraController.RaycastForFirst(out rh); //maybe replace with single ray with custom layer?

        var frame = rh.transform.GetComponentInParent<CoordinatePlane>();
        if (!frame) return false; // Not hovering over a frame
        var frameNum = frame.layerNum;
        var hitPoint = rh.point;

        var spawnPos = frame.WorldToLocal(hitPoint);
        Vector2 localCoordinates = frame.SnapToGrid(spawnPos);

        var nodeBackendID = backend.PlaceNode(nodeType, frameNum, localCoordinates, energyType);
        if (nodeBackendID != null)
        {
            frame.PlaceNode(nodeType, spawnPos, out var newNode, energyType);
          newNode.GetComponent<NodeVisual>().backendID = nodeBackendID.Value;
            return true;
        }


        return false;
    }

    private bool SpawnOnHoveredFrame(NodeDTO nodeDto, EnergyType energyType)
    {
        RaycastHit rh;
        cameraController.RaycastForFirst(out rh); //maybe replace with single ray with custom layer?

        var frame = rh.transform.GetComponentInParent<CoordinatePlane>();
        if (frame == null) return false; // Not hovering over a frame

        var hitPoint = rh.point;
        if (frame != null)
        {
            var spawnPos = frame.WorldToLocal(hitPoint);
            if (frame.PlaceNode(nodeDto, spawnPos, out var newNode, energyType)) return true;
        }


        return false;
    }

    private void DeleteNodeManually()
    {
        RaycastHit rh;
        cameraController.RaycastForFirst(out rh); //maybe replace with single ray with custom layer?

        var node = rh.transform.GetComponentInParent<NodeVisual>();
        if (!node) return;
        if (backend.DeleteNode(node.backendID)) Destroy(node.gameObject);
    }

    public GUID? isValidConduit(NodeVisual a, NodeVisual b)
    {
        if (!a || !b || a == b) return null;
        return backend.LinkNodes(a.backendID, b.backendID);
    }

    public CoordinatePlane GetCoordinatePlane(int startPosLayer)
    {
        return layerToCoordinatePlane.ContainsKey(startPosLayer)
            ? layerToCoordinatePlane[startPosLayer]
            : null ;
    }
}