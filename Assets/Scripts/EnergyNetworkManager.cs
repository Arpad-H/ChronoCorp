 using System.Collections.Generic;
 using UnityEngine;

public class EnergyNetworkManager : MonoBehaviour
{
    //TODO gets replaced by backend link later
 
    public List<Node> presentNodes = new List<Node>();
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

    public void AddNode(Node node)
    {
        presentNodes.Add(node);
    }
}
