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
        if (!temporalLayerStack) temporalLayerStack = FindObjectOfType<TemporalLayerStack>();
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
        backend.tick(fixedTickCount, this);
        fixedTickCount++;
    }


    public void GameOver(string reason)
    {
        Time.timeScale = 0f; // Pause game
        Debug.LogError($"GAME OVER: {reason}");
        UIManager.Instance.ShowGameOver(reason);
    }

    public bool PlaceNodeVisual(GUID id, NodeDTO nodeDto, int layerNum, Vector2 cellPos, EnergyType energyType)
    {
        var frame = GetCoordinatePlane(layerNum);
        NodeVisual nv = frame.PlaceNodeFromBackend(nodeDto, cellPos, energyType);
        if (nv)
        {
            nv.backendID = id;
            nodeVisuals.Add(id, nv);
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
        nodeVisuals[id].UpdateHealthBar((float)(currentValue - minValue) / (maxValue - minValue));
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
            frame.PlaceNode(nodeType, spawnPos, nodeBackendID.Value, energyType);

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

        var spawnPos = frame.WorldToLocal(hitPoint);
        GUID? nodeBackendID = backend.PlaceNode(nodeDto, frame.layerNum, spawnPos, energyType);
        GUID id;
        if (nodeBackendID.HasValue) id = nodeBackendID.Value;
        else return false;
        NodeVisual node = frame.PlaceNode(nodeDto, spawnPos, id, energyType);
        if (node)
        {
            nodeVisuals.Add(id, node);
            return true;
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
            : null;
    }

    public bool TryDrop(NodeDTO nodeDTO)
    {
        return SpawnOnHoveredFrame(nodeDTO, EnergyType.WHITE);
    }
}