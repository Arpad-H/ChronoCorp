// TimeLayerData.cs
using UnityEngine;
using System.Collections.Generic;

// These are plain C# classes, not MonoBehaviours.

[System.Serializable]
public class NodeData
{
    public int id;
    public Vector3 position;
    public bool isSource;
    public float energySupply;
    public float energyDemand;

    // Simulation state for this data-node
    public float currentEnergy;

    public NodeData(Node node)
    {
        this.id = node.id;
        this.position = node.transform.position;
        this.isSource = node.isSource;
        this.energySupply = node.energySupply;
        this.energyDemand = node.energyDemand;
        this.currentEnergy = node.currentEnergy;
    }
}

[System.Serializable]
public class ConduitData
{
    public int id;
    public int nodeA_id;
    public int nodeB_id;

    public ConduitData(Conduit conduit)
    {
        this.id = conduit.id;
        this.nodeA_id = conduit.nodeA.id;
        this.nodeB_id = conduit.nodeB.id;
    }
}

[System.Serializable]
public class TimeLayerState
{
    public int layerIndex; // e.g., 0 is t-1, 1 is t-2
    public List<NodeData> nodes = new List<NodeData>();
    public List<ConduitData> conduits = new List<ConduitData>();

    public float totalNetworkSupply = 0f;
    public float totalNetworkDemand = 0f;
    public float networkEfficiency = 1f;

    public TimeLayerState(int index)
    {
        this.layerIndex = index;
    }

    // Run a simulation step on this data
    public void SimulateStep()
    {
        totalNetworkSupply = 0f;
        totalNetworkDemand = 0f;

        foreach (NodeData node in nodes)
        {
            if (node.isSource)
                totalNetworkSupply += node.energySupply;
            else
                totalNetworkDemand += node.energyDemand;
        }

        if (totalNetworkDemand <= 0)
            networkEfficiency = 1f;
        else
            networkEfficiency = Mathf.Clamp01(totalNetworkSupply / totalNetworkDemand);

        // Update the data-nodes
        foreach (NodeData node in nodes)
        {
            if (node.isSource)
            {
                node.currentEnergy = node.energySupply;
            }
            else
            {
                node.currentEnergy = node.energyDemand * networkEfficiency;
            }
        }
    }
}