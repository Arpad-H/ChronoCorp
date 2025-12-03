// Conduit.cs

using System;
using System.Collections.Generic;
using NodeBase;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(LineRenderer))]
public class ConduitVisual : MonoBehaviour
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
        lineRenderer.positionCount = 3;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
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
        SetConduitEnergyType();
    }

    private void SetConduitEnergyType()
    {
        EnergyType energyType = nodeVisualA.energyType == EnergyType.WHITE ? nodeVisualB.energyType :nodeVisualA.energyType;
        Color color = energyType.ToColor();
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }
    
    public void StartNewConduitAtNode(NodeVisual nodeVisual)
    {
        if (nodeVisualA != null) return; // Don't start a new drag if one is active
        SetStartNode(nodeVisual);
    }
    
    
    public void SetPreviewPosition(Vector3 lineEnd)
    {
        Vector3 A = nodeVisualA.transform.position;
        Vector3 B = lineEnd;
        List<Vector3> path = new List<Vector3>();
        path.Add(A);
           
        bool horizontalFirst = Mathf.Abs(A.x - B.x) > Mathf.Abs(A.y - B.y);

        if (horizontalFirst)
        {
            // Horizontal → Vertical
            path.Add(new Vector3(B.x, A.y, A.z));
        }
        else
        {
            // Vertical → Horizontal
            path.Add(new Vector3(A.x, B.y, A.z));
        }

        path.Add(B);

        lineRenderer.SetPosition(0, path[0]);
        lineRenderer.SetPosition(1, path[1]);
        lineRenderer.SetPosition(2, path[2]);
    }

    public void Reset()
    {
        nodeVisualA  = null;
        nodeVisualB = null;
        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.zero);
        lineRenderer.SetPosition(2, Vector3.zero); }
}