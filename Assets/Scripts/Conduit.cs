// Conduit.cs

using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Conduit : MonoBehaviour
{
    
    public int id;
    public Node nodeA;
    public Node nodeB;
    private Vector3 dragPosition;
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
    }
    public void SetStartNode(Node node)
    {
        nodeA = node;
        lineRenderer.SetPosition(0, node.GetAttachPosition());
    }
    public void UpdateDragPosition(Vector3 position)
    {
        dragPosition = position;
        lineRenderer.SetPosition(1, dragPosition);
    }
    public void FinalizeConduit(Node node)
    {
        nodeB = node;
        lineRenderer.SetPosition(1, node.GetAttachPosition());
    }

    void Update()
    {
        // if (nodeA != null && nodeB != null)
        // {
        //     lineRenderer.SetPosition(0, nodeA.transform.position);
        //     lineRenderer.SetPosition(1, nodeB.transform.position);
        // }
    }
}