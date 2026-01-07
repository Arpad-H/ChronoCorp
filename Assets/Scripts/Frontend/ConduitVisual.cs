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
  //  public LineRenderer lineRenderer;
    public SplineContainer splineContainer;
    public SplineExtrude splineExtrudeRound;
    public SplineExtrude splineExtrudesquare;
    private Spline spline;
    public GUID backendID;
    public String debugInfo;
    
    
    public float pos = 0f;
    public MeshRenderer mr;
    public Material mat;
    void Awake()
    { 
        mat = mr.material;
        splineContainer = GetComponent<SplineContainer>();
        spline = splineContainer.Splines[0];
        // lineRenderer = GetComponent<LineRenderer>();
        // lineRenderer.positionCount = 3;
        // lineRenderer.startWidth = 0.1f;
        // lineRenderer.endWidth = 0.1f;
    }

    public void SetStartNode(NodeVisual nodeVisual)
    {
        nodeVisualA = nodeVisual;
        // lineRenderer.SetPosition(0, nodeVisual.GetAttachPosition());
    }

    public void FinalizeConduit(NodeVisual nodeVisual, GUID newBackendID)
    {
        backendID = newBackendID;
        nodeVisualB = nodeVisual;
        SetPreviewPosition(nodeVisual.GetAttachPosition());
        SetConduitEnergyType();
    }

    private void SetConduitEnergyType()
    {
        EnergyType energyType = nodeVisualA.energyType == EnergyType.WHITE
            ? nodeVisualB.energyType
            : nodeVisualA.energyType;
        Color color = energyType.ToColor();
        // lineRenderer.startColor = color;
        // lineRenderer.endColor = color;
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
       

        if (A.y - B.y < 0.1f && A.y - B.y > -0.1f) //same time slice
        {
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

            spline.Clear();
            // if (!nodeVisualA.isSource)
            // {
            //    path.Reverse();
            // }
            for (int i = 0; i < path.Count; i++)
            {
                BezierKnot knot = new BezierKnot(path[i]);
                // Tangents zero = straight, sharp bends (no curvature)
                knot.TangentIn = Vector3.zero;
                knot.TangentOut = Vector3.zero;
                spline.Add(knot);
            }
        }
        else //different time slices
        {
            path.Clear();
           // lineRenderer.positionCount = 4;
            if (A.y < B.y)
            {
                path.Add(A);
                path.Add(new Vector3(A.x, B.y + 1, A.z));
                path.Add(new Vector3(B.x, B.y + 1, B.z));
                path.Add(B);
            }
            else
            {
                path.Add(A);
                path.Add(new Vector3(A.x, A.y + 1, A.z));
                path.Add(new Vector3(B.x, A.y + 1, B.z));
                path.Add(B);
            }

            spline.Clear();
// BASE A – tangent must be flat!
            Vector3 tangentA = (B - A);
            tangentA.y = 0;
            tangentA.Normalize();

// BASE B – tangent must be flat!
            Vector3 tangentB = (A - B);
            tangentB.y = 0;
            tangentB.Normalize();
            float scale = Vector3.Distance(A, B) / 3f;
            BezierKnot knotA = new BezierKnot(
                A,
                -tangentA * scale,
                tangentA * scale,
                Quaternion.LookRotation(tangentA, Vector3.up)
            );

            BezierKnot knotB = new BezierKnot(
                B,
                -tangentB * scale,
                tangentB * scale,
                Quaternion.LookRotation(tangentB, Vector3.up)
            );
            float arcHeight = 2f;
            Vector3 mid = (A + B) * 0.5f;
            mid.y += arcHeight;
            
            BezierKnot knotMid = new BezierKnot(
                mid,
                -tangentB * scale,
                tangentB * scale,
                Quaternion.LookRotation(tangentB, Vector3.up)
            );


                spline.Add(knotA);
                spline.Add(knotMid);
                spline.Add(knotB);
                
           
        }
        // lineRenderer.positionCount = path.Count;
        // lineRenderer.SetPositions(path.ToArray());
    }

    Quaternion BuildStableRotation(Vector3 tangent)
    {
        tangent.Normalize();

        // Remove the component of up that points in tangent direction
        Vector3 upProjected = Vector3.ProjectOnPlane(Vector3.up, tangent).normalized;

        // If tangent is vertical, fallback
        if (upProjected.sqrMagnitude < 0.0001f)
            upProjected = Vector3.forward;

        return Quaternion.LookRotation(tangent, upProjected);
    }

    public void Update()
    {
        debugInfo = backendID.ToString();
        mat.SetFloat("_bulgePos", pos); // 0–1
    }

    public void Reset()
    {
        nodeVisualA = null;
        nodeVisualB = null;
        // lineRenderer.SetPosition(0, Vector3.zero);
        // lineRenderer.SetPosition(1, Vector3.zero);
        // lineRenderer.SetPosition(2, Vector3.zero);
    }
}