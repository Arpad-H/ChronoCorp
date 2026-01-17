// Conduit.cs

using System;
using System.Collections.Generic;
using System.Linq;
using Frontend.UIComponents;
using Interfaces;
using NodeBase;
using Unity.VisualScripting;
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
    public GameObject bridgePrefab;
    private bool enoughBridgesToFinish = true;
    private Vector3 dragPosition;
    private List<Vector3> path = new List<Vector3>();
    private bool sameLayerConnection = false;
    //  public LineRenderer lineRenderer;
    public SplineContainer splineContainer;
    private SplineExtrude splineExtrude;
    private MeshCollider meshCollider;
    public ConduitVisualizer conduitVisualizer;
    private Spline spline;
    public GUID backendID;
 
    private float conduitLength;
    public int bridgesBuilt = 0;
    public Renderer renderer;
    private Material pipeMaterial;
    public float[] positions = {}; 
    public List<GameObject> bridges;
   
    public Color invalidColor;
    public Color previewColor;
    public Color validColor;

    void Awake()
    { 
        
        pipeMaterial = renderer.material;
        splineContainer = GetComponent<SplineContainer>();
        splineExtrude = GetComponent<SplineExtrude>();
        meshCollider = GetComponent<MeshCollider>();
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

    public bool FinalizeConduit(NodeVisual nodeVisual, GUID newBackendID)
    {
        Direction dir = CalculateAttachDircection();
        SetPreviewPosition(nodeVisual.GetAttachPosition(dir), GameFrontendManager.Instance.temporalLayerStack.GetLayerByNum(nodeVisual.layerNum));
        if (!enoughBridgesToFinish) return false;
        backendID = newBackendID;
        targetNodeVisual = nodeVisual;
        SetConduitEnergyType();
        sourceNodeVisual.AddConnectedConduit(this,dir);
        targetNodeVisual.AddConnectedConduit(this,dir);
        conduitLength = spline.GetLength();
        if (sameLayerConnection)
        {
            Vector2Int[] cells = GetCellsOfConnection();
            int newLength = cells.Length - 2;
            Vector2Int[] trimmedCells = new Vector2Int[newLength];
            Array.Copy(cells, 1, trimmedCells, 0, newLength);
            planeA.AddCellsOccupiedByConduits(trimmedCells);
            GameFrontendManager.Instance.ConsumeInventoryItem(InventoryItem.BRIDGE, bridgesBuilt);
        }
        //path.Clear();
        return true;
    }

    private Direction CalculateAttachDircection()
    {
        Vector3 lastSegment;
        Vector3 secondLastSegment;
        if (sourceNodeVisual is Generator)
        {
            lastSegment = path[0];
            secondLastSegment = path[1];
        }
        else
        {
            lastSegment = path.Last();
            secondLastSegment = path[path.Count - 2];
        }
        
        Vector3 direction = (lastSegment - secondLastSegment).normalized;
        Direction attachDirection = Direction.Down; // Default
        float maxDot = -Mathf.Infinity;
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            Vector3 dirVector = dir.DirectionToVector3();
            float dot = Vector3.Dot(direction, dirVector);
            if (dot > maxDot)
            {
                maxDot = dot;
                attachDirection = dir;
            }
        }
        Debug.Log("Attach Direction: " + attachDirection);
        return attachDirection; 
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
        pipeMaterial.SetColor("_Color", color1);
        pipeMaterial.SetColor("_Color2", color2);
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
            path.Add(A);
            path.Add(mid);
            path.Add(B);
            

        }
        
    }

    private void SplineFromPath()
    {
        if (path.Count>= 2)
        {
            splineExtrude.enabled = true;
            meshCollider.enabled = true;
        }
        else
        {
            splineExtrude.enabled = false;
            meshCollider.enabled = false;
        }
        foreach (var bridge in bridges)
        {
            Destroy(bridge);
        }
        spline.Clear();
        // planeA.occupiedPositions.Add(sourceNodeVisual.GetAttachPosition(Direction.Down)); //TODO check which direction ctrl f for direction bcs hardcoded down appears at 3 spots incl this one
        for (int i = 0; i < path.Count; i++)
        {
            BezierKnot knot = new BezierKnot(path[i]);
            // Tangents zero = straight, sharp bends (no curvature)
            knot.Rotation = Quaternion.identity;
            knot.TangentIn = Vector3.zero;
            knot.TangentOut = Vector3.zero;
            spline.Add(knot);
            //  Vector3 planeLocalPos = planeA.ToPlaneLocal(planeA.WorldToLocal(path[i]));
            //       if (!planeA.IsPlaceOccupied(planeLocalPos)) planeA.occupiedPositions.Add(planeLocalPos);
        }

        //Place bridges on cells that are already occupied by other conduits
        if (sameLayerConnection)
        {
            //   Debug.Log("Finalized conduit on same layer with cells: " + string.Join(", ", GetCellsOfConnection()));
            //  Debug.Log("Occupied positions before placing bridges: " + string.Join(", ", planeA.occupiedPositions));
            var matches = planeA.GetMatchingOccupiedCellsByConduits(GetCellsOfConnection());
            bridgesBuilt = 0;
            foreach (var match in matches.AsEnumerable().Reverse())
            {
                Vector3 localPos = planeA.ToPlaneLocal(match);
                Transform nodeContainer = planeA.nodeContainer;
                GameObject obj = Instantiate(bridgePrefab, nodeContainer);
                Bridge bridge = obj.GetComponent<Bridge>();
                obj.transform.localPosition = localPos;
                int bridgeCount = GameFrontendManager.Instance.GetInvetoryCount(InventoryItem.BRIDGE);
                if (bridgesBuilt < bridgeCount)
                {
                    bridge.SetValidMaterial(true);
                    enoughBridgesToFinish = true;
                }
                else
                {
                    bridge.SetValidMaterial(false);
                    enoughBridgesToFinish = false;
                }
                bridges.Add(obj);
                bridgesBuilt++;
            }
        }
       
        ColorSplineIfValid();
    }

    private void ColorSplineIfValid()
    {
        if (enoughBridgesToFinish &&  planeA.IsPlaceOccupied(planeA.ToPlaneLocal(planeA.WorldToLocal(path.Last()))) is TimeRipple ripple)
        {
            pipeMaterial.SetColor("_Color", validColor);
            pipeMaterial.SetColor("_Color2", validColor);
        }
        else  if (enoughBridgesToFinish && GameFrontendManager.Instance.IsConnectionPathValid(sourceNodeVisual.layerNum, GetCellsOfConnection()))
        {
            pipeMaterial.SetColor("_Color", previewColor);
            pipeMaterial.SetColor("_Color2", previewColor);
        }
        else
        {
            pipeMaterial.SetColor("_Color", invalidColor);
            pipeMaterial.SetColor("_Color2", invalidColor);
        }

      //  Debug.Log("Finalized conduit on same layer with cells: " + string.Join(", ", GetCellsOfConnection()));
    }


    private void LateUpdate()
    {
        if (positions.Length == 0) AddBulge(-1); // Dummy value to avoid shader errors
        // Pass the array to the shader
        pipeMaterial.SetFloatArray("_BulgePositions", positions);
        // Tell the shader how many elements in the array to actually loop through
        pipeMaterial.SetInt("_BulgeCount", positions.Length);
        positions = new float[] { };
    }

    public void Reset()
    {
        if(planeA) CleanUpPlane();
        bridgesBuilt = 0;
        sourceNodeVisual = null;
        targetNodeVisual = null;
        path.Clear();
        spline.Clear();
    }

    private void CleanUpPlane()
    {
        foreach (var bridge in bridges)
        {
            Destroy(bridge);
        }
        bridges.Clear();
        if (!targetNodeVisual) return;
        Vector2Int[] cells = GetCellsOfConnection();
        planeA.RemoveOccupiedByConduitCells(cells);
      
    }

    public Vector2Int[] GetCellsOfConnection()
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        if (!sameLayerConnection)
        {
            Vector3 startPos =path [0];
            Vector3 endPos = path [path.Count - 1];
            Vector3 localStartPos = planeA.WorldToLocal(startPos);
            Vector2 startCell = planeA.ToPlaneLocal(localStartPos);
            Vector3 localEndPos = planeB.WorldToLocal(endPos);
            Vector2 endCell = planeB.ToPlaneLocal(localEndPos);
            localStartPos -= new Vector3(0.5f, 0.5f, 0); // Adjust for cell center
            localEndPos -= new Vector3(0.5f, 0.5f, 0); // Adjust for cell center
            int startX = Mathf.RoundToInt(localStartPos.x);
            int startY = Mathf.RoundToInt(localStartPos.y);
            int endX = Mathf.RoundToInt(localEndPos.x);
            int endY = Mathf.RoundToInt(localEndPos.y);
            cells.Add( new Vector2Int(startX, startY));
            cells.Add( new Vector2Int(endX, endY));
            return cells.ToArray();
        }
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
                conduitVisualizer.ReleaseItem(this);
                Destroy(deleteBtn.gameObject);
            }
          
        });
    }

    // public void setBulgePos(float pos)
    // {
    //     bulgePos = pos;
    // }

    public void InitializeNewConduit(GUID backendIdA, GUID backendIdB, GUID connectionId, Vector2Int[] cellsOfConnection) 
    {
        backendID = connectionId;
        sourceNodeVisual = GameFrontendManager.Instance.GetNodeVisual(backendIdA);
        targetNodeVisual = GameFrontendManager.Instance.GetNodeVisual(backendIdB);
        Direction dir = CalculateAttachDircection();
        SetPreviewPosition(targetNodeVisual.GetAttachPosition(dir), GameFrontendManager.Instance.temporalLayerStack.GetLayerByNum(targetNodeVisual.layerNum));
        SetConduitEnergyType();
        planeA = GameFrontendManager.Instance.temporalLayerStack.GetLayerByNum(sourceNodeVisual.layerNum);
        planeB = GameFrontendManager.Instance.temporalLayerStack.GetLayerByNum(targetNodeVisual.layerNum);
        sameLayerConnection = planeA.layerNum == planeB.layerNum;
        path = CellsToWorldPositions(cellsOfConnection);
        sourceNodeVisual.AddConnectedConduit(this,dir);
        targetNodeVisual.AddConnectedConduit(this,dir);
        conduitLength = spline.GetLength();
    }

    private List<Vector3> CellsToWorldPositions(Vector2Int[] cellsOfConnection)
    {
        List<Vector3> worldPositions = new List<Vector3>();
        if (!sameLayerConnection)
        {
            Vector2 startCell =cellsOfConnection [0];
            Vector2 endCell = cellsOfConnection[cellsOfConnection.Length - 1];
            Vector3 startWorldPos = planeA.GridToWorldPosition(startCell);
            Vector3 endWorldPos = planeB.GridToWorldPosition(endCell);
            worldPositions.Add(startWorldPos);
            worldPositions.Add(endWorldPos);
            return worldPositions;
        }
        foreach (var cell in cellsOfConnection)
        {
            Vector2 localPos = new Vector2(cell.x, cell.y ); 
            Vector3 worldPos = planeA.GridToWorldPosition(localPos);
            worldPositions.Add(worldPos);
        }
        return worldPositions;
    }

    public void AddBulge(float position)
    {
        Array.Resize(ref positions, positions.Length + 1);
        positions[positions.Length - 1] = position * conduitLength;
    }

    public void RemoveBulge()
    {
        // Currently does nothing; bulges are removed automatically each frame
    }
}