using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Generator : NodeVisual
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void RemoveConnectedConduit(ConduitVisual conduitVisual)
    {
        connectedConduits.Remove(conduitVisual);
        if (connectedConduits.Count == 0) ;
    }

    public override void AddConnectedConduit(ConduitVisual conduitVisual)
    {
        connectedConduits.Add(conduitVisual);
    }
}
