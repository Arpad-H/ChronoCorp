// Conduit.cs
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Conduit : MonoBehaviour
{
    public int id;
    public Node nodeA;
    public Node nodeB;
    public LineRenderer lineRenderer;

    void Awake()
    {
        id = GetInstanceID();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
    }

    // Call this to set up the conduit
    public void Initialize(Node a, Node b)
    {
        nodeA = a;
        nodeB = b;
        // Add self to the nodes' connection lists
        nodeA.connectedConduits.Add(this);
        nodeB.connectedConduits.Add(this);
    }

    void Update()
    {
        // Keep the line renderer drawn between its nodes
        if (nodeA != null && nodeB != null)
        {
            lineRenderer.SetPosition(0, nodeA.transform.position);
            lineRenderer.SetPosition(1, nodeB.transform.position);
        }
    }
}