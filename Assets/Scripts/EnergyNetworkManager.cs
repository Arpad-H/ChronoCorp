 using System.Collections.Generic;
 using UnityEngine;

public class EnergyNetworkManager : MonoBehaviour
{
    //TODO gets replaced by backend link later
 
    public List<NodeVisual> presentNodes = new List<NodeVisual>();
    public List<Conduit> presentConduits = new List<Conduit>();

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

    public void AddConduit(Conduit conduit)
    {
        presentConduits.Add(conduit);
    }

    public void AddNode(NodeVisual nodeVisual)
    {
        presentNodes.Add(nodeVisual);
    }
}
