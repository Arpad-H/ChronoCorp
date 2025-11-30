 using System.Collections.Generic;
 using UnityEngine;

public class EnergyNetworkManager : MonoBehaviour
{
    //TODO gets replaced by backend link later
 
    public List<NodeVisual> presentNodes = new List<NodeVisual>();
    public List<ConduitVisual> presentConduits = new List<ConduitVisual>();

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SimulateTemporalConnections();
    }
    void SimulateTemporalConnections()
    {
        // TODO
    }

    public void AddConduit(ConduitVisual conduitVisual)
    {
        presentConduits.Add(conduitVisual);
    }

    public void AddNode(NodeVisual nodeVisual)
    {
        presentNodes.Add(nodeVisual);
    }
}
