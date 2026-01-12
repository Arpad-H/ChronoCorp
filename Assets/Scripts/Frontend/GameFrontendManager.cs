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
    public event Action GeneratorDeleted;
    
    public static GameFrontendManager Instance;
    public CameraController cameraController;


    [Header("Layer Management")] public TemporalLayerStack temporalLayerStack;
    public float layerDuplicationTime = 60f;
    private Dictionary<int, CoordinatePlane> layerToCoordinatePlane = new();
    private Dictionary<GUID, NodeVisual> nodeVisuals = new();

    private IBackend backend; // Link to backend
    private EnergyPacketVisualizer energyPacketVisualizer;

    private long fixedTickCount;

    public StabilityBar stabilityBar;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
        if (!temporalLayerStack) temporalLayerStack = FindObjectOfType<TemporalLayerStack>();
       
    }

    private void Start()
    {
        backend = new BackendImpl(this);
        if (energyPacketVisualizer == null) energyPacketVisualizer = FindObjectOfType<EnergyPacketVisualizer>();
        if (cameraController == null) cameraController = FindObjectOfType<CameraController>();

        //get all existing layers in scene
        var existingLayers = FindObjectsOfType<CoordinatePlane>();
        foreach (var layer in existingLayers) layerToCoordinatePlane[layer.layerNum] = layer;
    }

    private void Update()
    {
    }

    private void FixedUpdate()
    {
        backend.tick(fixedTickCount, this);
        fixedTickCount++;
    }


    public void GameOver(string reason)
    {
        Time.timeScale = 0f; // Pause game
        UIManager.Instance.ShowGameOver(reason);
    }

    //Spawn Nodes from backend
    public bool PlaceNodeVisual(GUID id, NodeDTO nodeDto, int layerNum, Vector2 cellPos, EnergyType energyType)
    {
        var frame = GetCoordinatePlane(layerNum);
        NodeVisual nv = frame.PlaceNodeFromBackend(nodeDto, cellPos, energyType);
        if (nv)
        {
            nv.backendID = id;
            nv.layerNum = layerNum;
            nodeVisuals.Add(id, nv);
            Debug.Log("Spawned "+id);
            return true;
        }

        return false;
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
        float percent = (float)(currentValue - minValue) / (maxValue - minValue);
        stabilityBar.UpdateStabilityBar(percent);
    }

    public void OnActivateStabilityMalus(StabilityMalusType stabilityMalusType)
    {
    }

    public void OnDeactivateStabilityMalus(StabilityMalusType stabilityMalusType)
    {
    }

    public bool AddTimeSlice(int sliceNum)
    {
        CoordinatePlane newLayer = temporalLayerStack.AddNewFrame();
        newLayer.layerNum = sliceNum;
        layerToCoordinatePlane[sliceNum] = newLayer;
        return true;
    }

    public void onNodeHealthChange(GUID id, int minValue, int maxValue, int currentValue)
    {
        NodeVisual nodeVisual = nodeVisuals[id];
        if (nodeVisual.GetType() == typeof(TimeRipple))
        {
            ((TimeRipple)nodeVisual).UpdateHealthBar((float)(currentValue - minValue) / (maxValue - minValue));
        }
    }

    //Spawn Nodes from inventory or other manual placement
    private bool SpawnOnHoveredFrame(NodeDTO nodeDto, EnergyType energyType)
    {
        RaycastHit rh;
        cameraController.RaycastForFirst(out rh); //maybe replace with single ray with custom layer?
        var frame = rh.transform.GetComponentInParent<CoordinatePlane>();
        if (frame == null) return false; // Not hovering over a frame

        var hitPoint = rh.point;

        var spawnPos = frame.WorldToLocal(hitPoint);
        GUID? nodeBackendID = backend.PlaceNode(nodeDto, frame.layerNum, spawnPos, energyType);
        GUID id;
        if (nodeBackendID.HasValue) id = nodeBackendID.Value;
        else return false;
        NodeVisual node = frame.PlaceNode(nodeDto, spawnPos, id, energyType);
        if (node)
        {
            nodeVisuals.Add(id, node);
            node.layerNum = frame.layerNum;
            return true;
        }

        return false;
    }
    public GUID? IsValidConduit(NodeVisual a, NodeVisual b, Vector2[] cellsOfConnection)
    {
        if (!a || !b || a == b) return null;
        return backend.LinkNodes(a.backendID, b.backendID, cellsOfConnection);
    }
    public bool IsConnectionPathValid(int layerNum,  Vector2[] cellsOfConnection)
    {
        return !backend.IsConnectionPathOccupied(layerNum,cellsOfConnection);
    }

    public CoordinatePlane GetCoordinatePlane(int startPosLayer)
    {
        return layerToCoordinatePlane.ContainsKey(startPosLayer)
            ? layerToCoordinatePlane[startPosLayer]
            : null;
    }

    public bool TryDrop(NodeDTO nodeDTO)
    {
        return SpawnOnHoveredFrame(nodeDTO, EnergyType.WHITE);
    }

    public bool DestroyNode(GUID nodeID)
    {
        layerToCoordinatePlane[nodeVisuals[nodeID].layerNum].RemoveNodeVisual(nodeVisuals[nodeID]);
        bool result = backend.DeleteNode(nodeID);
        if (result) GeneratorDeleted?.Invoke();
        return result;
    }

    public bool UnlinkConduit(GUID backendID)
    {
        return backend.UnlinkNodes(backendID);
    }

    public int GetInvetoryCount(NodeDTO item)
    {
       return backend.GetAmountPlaceable(item);
    }

    public bool GetValuesForStabilityMalusType(StabilityMalusType type, out int threshold)
    {
        return backend.getValuesForStabilityMalusType(type, out threshold);
    }
}