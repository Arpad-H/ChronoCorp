using System.Collections.Generic;
using Frontend.UIComponents;
using Interfaces;
using NaughtyAttributes;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using Util;

public class Generator : NodeVisual
{
    private int generatorTier = 1;
    private int maxTier = 4;
    public Material[] generatorTiers;
    public GameObject[] generatorTiersObject;
    public DecalProjector decalProjector;
    [Label("Up Direction")] public GameObject upConnector;
    [Label("Down Direction")] public GameObject downConnector;
    [Label("Left Direction")] public GameObject leftConnector;
    [Label("Right Direction")] public GameObject rightConnector;
    protected Dictionary<Direction, GameObject> dirToConnectPoint;
    void Start()
    {
        SetGeneratorTier(generatorTier);
        dirToConnectPoint = new Dictionary<Direction, GameObject>()
        {
            {Direction.Up, upConnector},
            {Direction.Down, downConnector},
            {Direction.Left, leftConnector},
            {Direction.Right, rightConnector}
        };
    }
    
    protected override void HandlePointerClick()
    {
        DeleteButton deleteBtn = UIManager.Instance.SpawnDeleteButton(transform.position + Vector3.up);
        deleteBtn.Init(() =>
        {
            if (GameFrontendManager.Instance.DestroyNode(backendID))
            {
                foreach (ConduitVisual conduit in new List<ConduitVisual>(connectedConduits))
                {
                    conduit.ConnectedNodeDestroyedConnection(this);
                }
                GameFrontendManager.Instance.AddToInventory(InventoryItem.UPGRADE_CARD, generatorTier -1);
                Destroy(this.gameObject);
                Destroy(deleteBtn.gameObject);
            }
        });
    }
    protected override void ShowInfoWindow(bool show)
    {
        if (GeneratorInfoWindow.Instance == null) return;

        if (show)
        {
            GeneratorInfoWindow.Instance.Show(
                this, 
                backendID, 
                generatorTier, 
                GetOutput(),
                connectedConduits.Count
            );
        }
        else
        {
            GeneratorInfoWindow.Instance.Hide();
        }
    }

    public float GetOutput()
    {
        return BalanceProvider.Balance.energyPacketRechargeAmount / BalanceProvider.Balance.energyPacketSpawnIntervalPerSecond;
    }

    public override void RemoveConnectedConduit(ConduitVisual conduitVisual)
    {
        
        foreach (var dir in isDirectionOccupied)
        {
            if (dir.Value == conduitVisual)
            {
                dirToConnectPoint[dir.Key].SetActive(false);
                isDirectionOccupied[dir.Key] = null;
                break;
            }
        }
        connectedConduits.Remove(conduitVisual);
        if (connectedConduits.Count == 0) ;
    }

    public override void AddConnectedConduit(ConduitVisual conduitVisual,Direction dir)
    {
        dirToConnectPoint[dir].SetActive(true);
        isDirectionOccupied[dir] = conduitVisual;
        connectedConduits.Add(conduitVisual);
    }
    private void SetGeneratorTier(int tier)
    {
        Material newMat = generatorTiers[tier - 1];
        decalProjector.material = newMat;
        generatorTiersObject[generatorTier-1].SetActive(false);
        generatorTier = tier;
        generatorTiersObject[tier-1].SetActive(true);
    
    }

    public void UpgradeNode()
    {
        if (generatorTier < maxTier)
        {
            SetGeneratorTier(generatorTier + 1);
        }
    }
    public int GetTier()
    {
        return generatorTier;
    }
}