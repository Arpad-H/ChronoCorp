using System.Collections.Generic;
using Frontend.UIComponents;
using NaughtyAttributes;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;

public class Generator : NodeVisual
{
    private int generatorTier = 1;
    private int maxTier = 4;
    public GameObject[] generatorTiers;
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

                Destroy(this.gameObject);
                Destroy(deleteBtn.gameObject);
            }
        });
    }

    // Update is called once per frame
  
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
        generatorTiers[generatorTier-1].SetActive(false);
        generatorTier = tier;
        generatorTiers[tier-1].SetActive(true);
    }

    public void UpgradeNode()
    {
        if (generatorTier < maxTier)
        {
            SetGeneratorTier(generatorTier + 1);
        }
    }
}
