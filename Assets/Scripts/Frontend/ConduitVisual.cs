// Conduit.cs

using System;
using System.Collections.Generic;
using System.Linq;
using Frontend.UIComponents;
using NodeBase;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.Splines;

[RequireComponent(typeof(LineRenderer))]
public class ConduitVisual : MonoBehaviour, IPointerClickHandler
{
    public NodeVisual sourceNodeVisual;
    public NodeVisual targetNodeVisual;
    public CoordinatePlane planeA;
    public CoordinatePlane planeB; //if seperate time slices
    private Vector3 dragPosition;
    private List<Vector3> path = new List<Vector3>();
    private bool sameLayerConnection = false;
    //  public LineRenderer lineRenderer;
    public SplineContainer splineContainer;
    public SplineExtrude splineExtrudeRound;
    public SplineExtrude splineExtrudesquare;
    private Spline spline;
    public GUID backendID;
    public String debugInfo;
    private float conduitLength;
    private float bulgePos = 0f; 
    public MeshRenderer mr;
    public Material mat;
    public Color invalidColor;
    public Color previewColor;
    void Awake()
    { 
        mat = mr.material;
        splineContainer = GetComponent<SplineContainer>();
        spline = splineContainer.Splines[0];
    }

   public void ConnectedNodeDestroyedConnection(NodeVisual nodeVisual)
    {
        if (sourceNodeVisual == nodeVisual)
        {
            sourceNodeVisual.RemoveConnectedConduit(this);
        }
        else if (targetNodeVisual == nodeVisual)
        {
            targetNodeVisual.RemoveConnectedConduit(this);
        }
        Destroy(this.gameObject);
    }

    public void FinalizeConduit(NodeVisual nodeVisual, GUID newBackendID)
    {
        backendID = newBackendID;
        targetNodeVisual = nodeVisual;
        SetPreviewPosition(nodeVisual.GetAttachPosition(), GameFrontendManager.Instance.temporalLayerStack.GetLayerByNum(nodeVisual.layerNum));
        SetConduitEnergyType();
        path.Clear();
        sourceNodeVisual.AddConnectedConduit(this);
        targetNodeVisual.AddConnectedConduit(this);
        conduitLength = spline.GetLength();
    }

    private void SetConduitEnergyType()
    {
        EnergyType energyType = EnergyType.WHITE;
        if (sourceNodeVisual is TimeRipple rippleA)
            energyType = rippleA.energyType;
        else if (targetNodeVisual is TimeRipple rippleB)
            energyType = rippleB.energyType;
        
        Color color2 = energyType.ToColor();
        float factor = Mathf.Pow(2,3);
        Color color1 = new Color(color2.r*factor, color2.g*factor, color2.b*factor, 1f);
        mat.SetColor("_Color", color1);
        mat.SetColor("_Color2", color2);
    }

    public void StartNewConduitAtNode(NodeVisual nodeVisual,CoordinatePlane plane)
    {
        if (sourceNodeVisual != null) return; // Don't start a new drag if one is active
        sourceNodeVisual = nodeVisual;
        planeA = plane;
    }


    public void SetPreviewPosition(Vector3 lineEnd, CoordinatePlane layerB)
    {
        int layerA = sourceNodeVisual.layerNum;
        Vector3 A = sourceNodeVisual.transform.position;
        Vector3 B = lineEnd;
        
        if (layerA == layerB.layerNum) //same time slice
        {
            sameLayerConnection = true;
            Vector3 snappedLocalPos = layerB.WorldToLocal(lineEnd);
            Vector3 snappedPos = layerB.GridToWorldPosition(snappedLocalPos);
            
            if(layerB.IsPlaceOccupied(layerB.ToPlaneLocal(snappedLocalPos)))
            {
             // return; works but need to handle overshooting occupied nodes
            }
            if (path.Count == 0)
            {
                path.Add(snappedPos);
                SplineFromPath();
                return;
            }
            Vector3 lastPos = path.Last();
            if (snappedPos == lastPos) return;
            if (path.Contains(snappedPos))
            {
                int index = path.IndexOf(snappedPos);
                path.RemoveRange(index + 1, path.Count - (index + 1));
            }
            else
            {
                // FILL THE GAP: Calculate the distance in grid units
                // Assuming your grid size is 1.0f; if not, divide by your grid cell size.
                float dist = Vector3.Distance(lastPos, snappedPos);
                float gridSize = 1f; // Replace with your layerB grid size

                if (dist > gridSize + 0.2f)
                {
                    // Calculate how many steps we need to fill the gap
                    int steps = Mathf.CeilToInt(dist / gridSize);
                    for (int i = 1; i <= steps; i++)
                    {
                        float t = (float)i / steps;
                        Vector3 intermediatePos = Vector3.Lerp(lastPos, snappedPos, t);
                
                        // Snap the intermediate result to the grid
                        Vector3 finalSnapped = layerB.GridToWorldPosition(layerB.WorldToLocal(intermediatePos));
                
                        if (!path.Contains(finalSnapped))
                        {
                            path.Add(finalSnapped);
                        }
                    }
                }
                else
                {
                    path.Add(snappedPos);
                }
            } 

          
            SplineFromPath();
        }
        else //different time slices
        {
            sameLayerConnection = false;
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

    private void SplineFromPath()
    {
        spline.Clear();
        planeA.occupiedPositions.Add(sourceNodeVisual.GetAttachPosition());
        for (int i = 0; i < path.Count; i++)
        {
            BezierKnot knot = new BezierKnot(path[i]);
            // Tangents zero = straight, sharp bends (no curvature)
            knot.Rotation = Quaternion.identity;
            knot.TangentIn = Vector3.zero;
            knot.TangentOut = Vector3.zero;
            spline.Add(knot);
            Vector3 planeLocalPos = planeA.ToPlaneLocal(planeA.WorldToLocal(path[i]));
            if (!planeA.IsPlaceOccupied(planeLocalPos)) planeA.occupiedPositions.Add(planeLocalPos);
        }

        if (sameLayerConnection && GameFrontendManager.Instance.IsConnectionPathValid(sourceNodeVisual.layerNum, GetCellsOfConnection()))
        {
            mat.SetColor("_Color2", previewColor);
        }
        else
        {
            mat.SetColor("_Color2", invalidColor);
        }
    }

   
    public void Update()
    {
        debugInfo = backendID.ToString();
        mat.SetFloat("_bulgePos", bulgePos*conduitLength); 
    }

    public void Reset()
    {
        sourceNodeVisual = null;
        targetNodeVisual = null;
        path.Clear();
    }

    public Vector2Int[] GetCellsOfConnection()
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        foreach (var worldPos in path)
        {
            Vector3 localPos = planeA.WorldToLocal(worldPos);
            Vector2 cell = planeA.ToPlaneLocal(localPos);
            Vector2Int cellInt = new Vector2Int((int)Mathf.Floor(cell.x), (int)Mathf.Floor(cell.y));
            if (!cells.Contains(cellInt))
            {
                localPos -= new Vector3(0.5f, 0.5f, 0); // Adjust for cell center
                int x = Mathf.RoundToInt(localPos.x);
                int y = Mathf.RoundToInt(localPos.y);
                cells.Add(new Vector2Int(x, y));
            }
        }
        return cells.ToArray();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        Vector3 clickPosWorld = eventData.pointerCurrentRaycast.worldPosition;
        SpawnDeleteButton( clickPosWorld);
    }
    
    private void SpawnDeleteButton(Vector3 clickPosWorld)
    {
      
        DeleteButton deleteBtn = UIManager.Instance.SpawnDeleteButton(clickPosWorld + Vector3.up);
        deleteBtn.Init(() =>
        {
            if(GameFrontendManager.Instance.UnlinkConduit(backendID))
            {
                sourceNodeVisual.RemoveConnectedConduit(this);
                targetNodeVisual.RemoveConnectedConduit(this);
                Destroy(this.gameObject);
                Destroy(deleteBtn.gameObject);
            }
          
        });
    }
    public void setBulgePos(float pos)
    {
        bulgePos = pos;
    }
}