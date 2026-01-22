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
    [Serializable]
    public enum GameState
    {
        PLAYING,
        PAUSED,
        GAMEOVER
    }
    public event Action GeneratorDeleted;
    public event Action InventoryChanged;
    public event Action<Guid> BackendDeletesConnection;
    public event Action<Guid,Guid,Guid,Vector2Int[]> BackendCreatesConnection;
    
    public static GameFrontendManager Instance;
    public CameraController cameraController;
   

    [Header("Layer Management")] public TemporalLayerStack temporalLayerStack;
    public float layerDuplicationTime = 60f;
  
    private Dictionary<Guid, NodeVisual> nodeVisuals = new();

    private IBackend backend; // Link to backend
    private EnergyPacketVisualizer energyPacketVisualizer;

    private long fixedTickCount;

    public StabilityBar stabilityBar;
    [SerializeField] private GameState gameState = GameState.PAUSED;
    

    private void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        }
        else {
            Instance = this;
        }
        if (energyPacketVisualizer == null) energyPacketVisualizer = FindObjectOfType<EnergyPacketVisualizer>();
        if (cameraController == null) cameraController = FindObjectOfType<CameraController>();
        if (!temporalLayerStack) temporalLayerStack = FindObjectOfType<TemporalLayerStack>();
        backend = new BackendImpl(this);
    }

    private void Start()
    {
       
    }

    private void FixedUpdate()
    {
    
        if (gameState == GameState.PAUSED || gameState == GameState.GAMEOVER) return;
        backend.tick(fixedTickCount, this);
        fixedTickCount++;
    }


    public void GameOver(string reason)
    {
       gameState = GameState.GAMEOVER;
        
        AudioManager.Instance.StopBackgroundMusic();
        AudioManager.Instance.PlayLossAudio();
        UIManager.Instance.ShowGameOver(reason);
    }

    //Spawn Nodes from backend
    public bool PlaceNodeVisual(Guid id, NodeDTO nodeDto, int layerNum, Vector2 cellPos, EnergyType energyType)
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

    public void DeleteConnection(Guid connectionId)
    {
        BackendDeletesConnection?.Invoke(connectionId);
    }
    // Called by the backend to delete a node visual
    public void DeleteNode(Guid nodeId)
    {
        CoordinatePlane layer = temporalLayerStack.GetLayerByNum(nodeVisuals[nodeId].layerNum);
        layer.RemoveNodeVisual(nodeVisuals[nodeId]);
        nodeVisuals.Remove(nodeId);
        GeneratorDeleted?.Invoke();
    }

    // Called when player deletes a node
    public bool DestroyNode(Guid nodeID)
    {
        CoordinatePlane layer = temporalLayerStack.GetLayerByNum(nodeVisuals[nodeID].layerNum);
        layer.RemoveNodeVisual(nodeVisuals[nodeID]);
        bool result = backend.DeleteNode(nodeID);
        if (result)
        {
            AudioManager.Instance.PlayPlayerActionSuccessSound();
            GeneratorDeleted?.Invoke();
        }
        nodeVisuals.Remove(nodeID);
        return result;
    }

    public void CreateConnection(Guid backendIdA, Guid backendIdB, Guid connectionId, Vector2Int[] cellsOfConnection)
    {
        BackendCreatesConnection?.Invoke(backendIdA, backendIdB, connectionId, cellsOfConnection);
    }

    public void SpawnEnergyPacket(Guid guid, EnergyType energyType)
    {
        energyPacketVisualizer.SpawnEnergyPacket(guid, energyType);
    }

    public void DeleteEnergyPacket(Guid guid)
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
            UIManager.Instance.ShowUpgradeChoiceMenu();
            BalanceProvider.Balance.nodeSpawnIntervalPerSecond += BalanceProvider.Balance.layerModifierToNodeSpawnInterval;
            return true;
        }
        return false;
    }

    public void onNodeHealthChange(Guid id, int minValue, int maxValue, int currentValue)
    {
        NodeVisual nodeVisual = nodeVisuals[id];
        if (nodeVisual.GetType() == typeof(TimeRipple))
        {
            ((TimeRipple)nodeVisual).UpdateHealthBar((float)(currentValue - minValue) / (maxValue - minValue));
        }
        if (nodeVisual.GetType() == typeof(BlackHole))
        {
            ((BlackHole)nodeVisual).UpdateHealthBar((float)(currentValue - minValue) / (maxValue - minValue));
        }
        if (nodeVisual.GetType() == typeof(Blockade))
        {
            ((Blockade)nodeVisual).UpdateHealthBar((float)(currentValue - minValue) / (maxValue - minValue));
        }
    }

    //Spawn Nodes from inventory or other manual placement
    private bool SpawnOnHoveredFrame(NodeDTO nodeDto, EnergyType energyType)
    {
        RaycastHit rh;
        cameraController.RaycastForFirst(out rh); 
       if (rh.collider == null) return false; // Not hovering over anything
        var frame = rh.transform.GetComponentInParent<CoordinatePlane>();
        if (frame == null) return false; // Not hovering over a frame

        var hitPoint = rh.point;

        var spawnPos = frame.WorldToLocal(hitPoint);
        Guid? nodeBackendID = backend.PlaceNode(nodeDto, frame.layerNum, spawnPos, energyType);
        Guid id;
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

    public Guid? IsValidConduit(NodeVisual a, NodeVisual b, Vector2Int[] cellsOfConnection,int bridgesBuilt)
    {
        if (!a || !b || a == b) return null;
        Guid? pathValid = backend.LinkNodes(a.backendID, b.backendID, cellsOfConnection, bridgesBuilt);
        if (!pathValid.HasValue)
        {
           AudioManager.Instance.PlayInvalidActionSound();
        }else AudioManager.Instance.PlayPlayerActionSuccessSound();
        return pathValid;
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
        bool success = false;
        switch (item)
        {
            case InventoryItem.GENERATOR:
            {
                 success = SpawnOnHoveredFrame(NodeDTO.GENERATOR, EnergyType.WHITE);
                 break;
            }

            case InventoryItem.UPGRADE_CARD:
            {
                success = UpgradeHoveredNode();
                break;
            }
        }
        if (success) AudioManager.Instance.PlayPlayerActionSuccessSound();
        else AudioManager.Instance.PlayInvalidActionSound();
        return success;
    }

    public void ConsumeInventoryItem(InventoryItem item, int amount = 1)
    {
        backend.AddItemToInventory(item, -amount);
        InventoryChanged?.Invoke();
    }

    private bool UpgradeHoveredNode()
    {
        RaycastHit rh;
        cameraController.RaycastForFirst(out rh); 
        var nodeVisual = rh.collider.GetComponent<Generator>(); //TODO Currently only generators can be upgraded
        if (nodeVisual == null) return false; // Not hovering over a node
        if (backend.upgradeGenerator(nodeVisual.backendID))
        {
            nodeVisual.UpgradeNode();
            AudioManager.Instance.PlayPlayerActionSuccessSound();
            AddToInventory(InventoryItem.UPGRADE_CARD,-1);
            return true;
        }
        else
        {
            AudioManager.Instance.PlayInvalidActionSound();
            return false;
        }
       
       
    }

    public bool UnlinkConduit(Guid backendID)
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

    public void UpgradeCardSelected(UpgradeCardData upgradeCard)
    {
        CardEffectEvaluator.ApplyEffect(upgradeCard);
    }

    public float GetEnergyPacketProgress(Guid guid, out Guid? sourceNode, out Guid? targetNode, out Guid? conduitID)
    {
        return backend.GetEnergyPacketProgress(guid, out sourceNode, out targetNode, out conduitID);
    }
    public NodeVisual GetNodeVisual(Guid nodeID) 
    {
        return nodeVisuals[nodeID];
    }

    public void AddToInventory(InventoryItem item, float amount)
    {
        backend.AddItemToInventory(item, (int)amount);
        InventoryChanged?.Invoke();
    }

    public void EndTutorial()
    {
       gameState = GameState.PLAYING;
    }
    public void SetGameState(GameState state)
    {
        gameState = state;
    }

    public void AddScore(int scorePerInterval)
    {
        UIManager.Instance.AddScore(scorePerInterval);
    }
}