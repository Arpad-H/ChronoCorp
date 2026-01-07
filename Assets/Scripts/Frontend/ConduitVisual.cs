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
    }

    public void SetStartNode(NodeVisual nodeVisual)
    {
        nodeVisualA = nodeVisual;
      
    }

    public void FinalizeConduit(NodeVisual nodeVisual, GUID newBackendID)
    {
        backendID = newBackendID;
        nodeVisualB = nodeVisual;
        SetPreviewPosition(nodeVisual.GetAttachPosition(), nodeVisualB.layerNum);
        SetConduitEnergyType();
    }

    private void SetConduitEnergyType()
    {
        EnergyType energyType = nodeVisualA.energyType == EnergyType.WHITE
            ? nodeVisualB.energyType
            : nodeVisualA.energyType;
        Color color = energyType.ToColor();
        mat.SetColor("_Color", color);
     
    }

    public void StartNewConduitAtNode(NodeVisual nodeVisual)
    {
        if (nodeVisualA != null) return; // Don't start a new drag if one is active
        SetStartNode(nodeVisual);
    }


    public void SetPreviewPosition(Vector3 lineEnd, int layerNumberB)
    {
        Vector3 A = nodeVisualA.transform.position;
        int layerA = nodeVisualA.layerNum;
        Vector3 B = lineEnd;
        List<Vector3> path = new List<Vector3>();
        if (layerA == layerNumberB) //same time slice
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
            spline.Clear();
            float height = 2f; // Height of the bulge
            float verticalStrength = 1f; // how aggressive the vertical drop is
            float tangentScale = 1f;  
            
            Vector3 up = Vector3.up;

            Vector3 mid = (A + B) * 0.5f + up * height;

            Vector3 flatDir = (B - A).normalized;

            float totalDist = Vector3.Distance(A, B);
            float halfDist = totalDist * 0.5f;
            Vector3 groundNormal = Vector3.up;
            Vector3 down = -groundNormal;

            Vector3 side = Vector3.Cross(flatDir, groundNormal).normalized;
            Vector3 verticalInPlane = Vector3.Cross(side, flatDir).normalized;

// ---- START KNOT ----
            BezierKnot startKnot = new BezierKnot(A);
            startKnot.TangentOut =
                verticalInPlane * verticalStrength * tangentScale;

// ---- MID KNOT ----
            BezierKnot midKnot = new BezierKnot(mid);
            midKnot.TangentIn  = -flatDir * halfDist * 0.5f;
            midKnot.TangentOut =  flatDir * halfDist * 0.5f;

// ---- END KNOT ----
            BezierKnot endKnot = new BezierKnot(B);
            endKnot.TangentIn =
                verticalInPlane * verticalStrength * tangentScale;
            spline.Add(startKnot);
            spline.Add(midKnot);
            spline.Add(endKnot);
        
         
          
                
           
        }



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
     
    }
}