// Conduit.cs

using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(LineRenderer))]
public class Conduit : MonoBehaviour
{
    
    public int id;
    [FormerlySerializedAs("nodeA")] public NodeVisual nodeVisualA;
    [FormerlySerializedAs("nodeB")] public NodeVisual nodeVisualB;
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
    public void Initialize(NodeVisual a, NodeVisual b)
    {
        // nodeA = a;
        // nodeB = b;
        SetStartNode(a);
        FinalizeConduit(b);
    }
    public void SetStartNode(NodeVisual nodeVisual)
    {
        nodeVisualA = nodeVisual;
        lineRenderer.SetPosition(0, nodeVisual.GetAttachPosition());
    }
    public void UpdateDragPosition(Vector3 position)
    {
        dragPosition = position;
        lineRenderer.SetPosition(1, dragPosition);
    }
    public void FinalizeConduit(NodeVisual nodeVisual)
    {
        nodeVisualB = nodeVisual;
        lineRenderer.SetPosition(1, nodeVisual.GetAttachPosition());
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