using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Generator : NodeVisual
{
    private int generatorTier = 1;
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
        generatorTier = tier;
        generatorTiers[tier-1].SetActive(true);
    }

    public void UpgradeNode()
    {
        SetGeneratorTier (generatorTier + 1);
    }
}
