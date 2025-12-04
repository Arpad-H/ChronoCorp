// Conduit.cs

using System;
using System.Collections.Generic;
using NodeBase;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Splines;

[RequireComponent(typeof(LineRenderer))]
public class ConduitVisual : MonoBehaviour
{
    [FormerlySerializedAs("nodeA")] public NodeVisual nodeVisualA;
    [FormerlySerializedAs("nodeB")] public NodeVisual nodeVisualB;
    private Vector3 dragPosition;
    public LineRenderer lineRenderer;
    public SplineContainer splineContainer;
    private Spline spline;
    public GUID backendID;
    public String debugInfo;

    void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
        spline = splineContainer.Splines[0];
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
    public void FinalizeConduit(NodeVisual nodeVisual,GUID newBackendID)
    {
        backendID = newBackendID;
        nodeVisualB = nodeVisual;
        SetPreviewPosition(nodeVisual.GetAttachPosition());
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
        lineRenderer.SetPositions(path.ToArray());
       
        spline.Clear();
        if (!nodeVisualA.isSource)
        {
           path.Reverse();
        }
        for (int i = 0; i < path.Count; i++)
        {
            BezierKnot knot = new BezierKnot(path[i]);
            // Tangents zero = straight, sharp bends (no curvature)
            knot.TangentIn = Vector3.zero;
            knot.TangentOut = Vector3.zero;
            spline.Add(knot);
        }

    }

    public void Update()
    {
        debugInfo = backendID.ToString();
    }

    public void Reset()
    {
        nodeVisualA  = null;
        nodeVisualB = null;
        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.zero);
        lineRenderer.SetPosition(2, Vector3.zero); }
}