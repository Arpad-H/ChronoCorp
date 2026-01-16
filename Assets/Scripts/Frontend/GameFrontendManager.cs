// GameManager.cs

using System;
using System.Collections.Generic;
using Backend.Simulation.World;
using Frontend;
using Interfaces;
using NodeBase;
using UnityEditor;
using UnityEngine;
using Util;

public class GameFrontendManager : MonoBehaviour, IFrontend
{
    public event Action GeneratorDeleted;
    public event Action InventoryChanged;
    public event Action<GUID> BackendDeletesConnection;
    public event Action<GUID,GUID,GUID,Vector2Int[]> BackendCreatesConnection;
    
    public static GameFrontendManager Instance;
    public CameraController cameraController;


    [Header("Layer Management")] public TemporalLayerStack temporalLayerStack;
    public float layerDuplicationTime = 60f;
  
    private Dictionary<GUID, NodeVisual> nodeVisuals = new();

    private IBackend backend; // Link to backend
    private EnergyPacketVisualizer energyPacketVisualizer;

    private long fixedTickCount;

    public StabilityBar stabilityBar;
    private bool gameOver = false;
    

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
    }

    private void Update()
    {
    }

    private void FixedUpdate()
    {
        if (gameOver) return;
        backend.tick(fixedTickCount, this);
        fixedTickCount++;
    }


    public void GameOver(string reason)
    {
        gameOver = true;
        
        AudioManager.Instance.StopBackgroundMusic();
        AudioManager.Instance.PlayLossAudio();
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
            return true;
        }

        return false;
    }

    public void DeleteConnection(GUID connectionId)
    {
        BackendDeletesConnection?.Invoke(connectionId);
    }

    public void CreateConnection(GUID backendIdA, GUID backendIdB, GUID connectionId, Vector2Int[] cellsOfConnection)
    {
        BackendCreatesConnection?.Invoke(backendIdA, backendIdB, connectionId, cellsOfConnection);
    }

    public void SpawnEnergyPacket(GUID guid, EnergyType energyType)
    {
        energyPacketVisualizer.SpawnEnergyPacket(guid, energyType);
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
        CoordinatePlane newLayer = temporalLayerStack.AddNewFrame(sliceNum);
        if (newLayer && sliceNum != 0)
        {
            UIManager.Instance.ShowUpgradeChoiceMenu(BalanceProvider.Balance.upgradeCards);
            return true;
        }
        return false;
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
        cameraController.RaycastForFirst(out rh); 
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
    public GUID? IsValidConduit(NodeVisual a, NodeVisual b, Vector2Int[] cellsOfConnection)
    {
        if (!a || !b || a == b) return null;
        return backend.LinkNodes(a.backendID, b.backendID, cellsOfConnection);
    }
    public bool IsConnectionPathValid(int layerNum,  Vector2Int[] cellsOfConnection)
    {
        return !backend.IsConnectionPathOccupied(layerNum,cellsOfConnection);
    }

    public CoordinatePlane GetCoordinatePlane(int layerNum)
    {
        return temporalLayerStack.GetLayerByNum(layerNum);
    }

    public bool TryDrop(InventoryItem item)
    {
        switch (item)
        {
            case InventoryItem.GENERATOR :
                return SpawnOnHoveredFrame(NodeDTO.GENERATOR, EnergyType.WHITE);
            case InventoryItem.UPGRADE_CARD : return UpgradeHoveredNode(); return false;
        }
        return false;
    }

    private bool UpgradeHoveredNode()
    {
        RaycastHit rh;
        cameraController.RaycastForFirst(out rh); 
        var nodeVisual = rh.collider.GetComponent<Generator>(); //TODO Currently only generators can be upgraded
        if (nodeVisual == null) return false; // Not hovering over a node

        nodeVisual.UpgradeNode();
        return true;
    }

    public bool DestroyNode(GUID nodeID)
    {
        CoordinatePlane layer = temporalLayerStack.GetLayerByNum(nodeVisuals[nodeID].layerNum);
        layer.RemoveNodeVisual(nodeVisuals[nodeID]);
        bool result = backend.DeleteNode(nodeID);
        if (result) GeneratorDeleted?.Invoke();
        return result;
    }

    public bool UnlinkConduit(GUID backendID)
    {
        return backend.UnlinkNodes(backendID);
    }

    public int GetInvetoryCount(InventoryItem item)
    {
        return backend.GetAmountPlaceable(item);
    }

    public bool GetValuesForStabilityMalusType(StabilityMalusType type, out int threshold)
    {
        return backend.getValuesForStabilityMalusType(type, out threshold);
    }

    public void UpgradeCardSelected(UpgradeData upgrade)
    {
        CardEffectEvaluator.ApplyEffect(upgrade);
    }

    public float GetEnergyPacketProgress(GUID guid, out GUID? sourceNode, out GUID? targetNode, out GUID? conduitID)
    {
        return backend.GetEnergyPacketProgress(guid, out sourceNode, out targetNode, out conduitID);
    }
    public NodeVisual GetNodeVisual(GUID nodeID) 
    {
        return nodeVisuals[nodeID];
    }

    public void AddToInventory(InventoryItem item, float amount)
    {
        backend.AddItemToInventory(item, (int)amount);
        InventoryChanged?.Invoke();
    }
}