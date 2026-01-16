using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using NodeBase;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class CoordinatePlane : MonoBehaviour
{
    
    public int layerNum = 0; //changed when bakcend spawns layer
     private IBackend _backend;
 
     private List<NodeVisual> nodes = new List<NodeVisual>();
     public List<Vector3> occupiedPositions = new List<Vector3>();
     
    [Header("Prefabs")]
    public GameObject nodePrefab;
    public GameObject generatorPrefab;
    public GameObject blackHolePrefab;
    public GameObject blockadePrefab;

    
    public Transform frameMesh; // scaled mesh
    public Transform nodeContainer; // unscaled coordinate space

    [Header("Grid Settings")]
    // public int gridWidth = 10;
    // public int gridHeight = 10; //currently determined by frameMesh scale
    public int cellWidth = 1;
    public int cellHeight = 1;
    private int numX, numY; // Number of cells in each direction
    private float maxX, maxY, minX, minY; // Max/min bounds (avoid placing outside frame)
    
        
   
    
    [Header("Spawn Effect")]
    public GameObject vfxPrefab;
    public MeshRenderer meshRenderer;
    public GameObject deco;
    
    [Header("decal")]
    public DecalProjector decalProjector;
    public Material tex1;
    public Material tex2;
    public Material tex3;
    public Material tex4;
    public Material texEmpty;
    
    void Awake()
    {
        Debug.Log("CoordinatePlane Awake");
        meshRenderer.enabled =false;
         deco.SetActive(false);
        if (!frameMesh) frameMesh = transform.Find("FrameMesh");
        if (!nodeContainer) nodeContainer = transform.Find("NodeContainer");
        // Calculate bounds
        Vector3 scale = frameMesh.localScale;
        numX = Mathf.CeilToInt(scale.x / cellWidth);
        numY = Mathf.CeilToInt(scale.y / cellHeight);
        // Calculate min/max local bounds of the grid
        minX = -scale.x / 2f;
        maxX = minX + numX * cellWidth;
        minY = -scale.y / 2f;
        maxY = minY + numY * cellHeight;
    }
    void Start()
    {
        StartCoroutine(DelaySpawn());
    }
    IEnumerator DelaySpawn()
    {
        yield return new WaitForSeconds(3);
        vfxPrefab.SetActive(false);
        meshRenderer.enabled = true;
        deco.SetActive(true);
        switch (layerNum)  //TODO fix discrepency between layerNum and texture index
        {
            case 0:
                decalProjector.material = tex1;
                break;
            case 1:
                decalProjector.material =  tex2;
                break;
            case 2:
                decalProjector.material = tex3;
                break;
            case 3:
                decalProjector.material = tex4;
                break;
            default:
                decalProjector.material = texEmpty;
                break;
        }
       
    }
  
    void Update()
    {
      
    }

    /// <summary>
    /// Converts plane coords (0→width, 0→height) into the nodeContainer local space
    /// </summary>
    public Vector3 ToPlaneLocal(Vector2 planeNonLocal)
    {
        Vector2 planePos = SnapToGrid(planeNonLocal);
        Vector3 scale = frameMesh.localScale;

        return new Vector3(
            planePos.x - scale.x / 2f,
            planePos.y - scale.y / 2f,
            -0.51f
        );
    }

    // /// <summary>
    // /// Instantiates a node inside the nodeContainer at plane coordinates.
    // /// </summary>
    public NodeVisual PlaceNode(NodeDTO nodeDTO, Vector3 planePos,GUID guid, EnergyType energyType)
    {
        NodeVisual nv = null;
        switch (nodeDTO)
        {
            case NodeDTO.RIPPLE:
            {
                TimeRipple timeRipple = PlaceTimeRipple(planePos, energyType); 
                if (timeRipple)
                {
                    timeRipple.backendID = guid;
                    timeRipple.SetEnergyType(energyType);
                  
                }
                nv = timeRipple;
                break;
            }
            case NodeDTO.GENERATOR:
            {
                Generator generator = PlaceGenerator(planePos); 
                if (generator)
                {
                    generator.backendID = guid;
                }
                nv = generator;
                break;
            }
            
        }
        if(nv) nodes.Add(nv);
        return nv;
    }
    
    public NodeVisual PlaceNodeFromBackend(NodeDTO nodeDTO, Vector2 planePos, EnergyType energyType)
    {
        switch (nodeDTO)
        {
            case  NodeDTO.RIPPLE: return PlaceTimeRipple(planePos, energyType); break;
            case NodeDTO.GENERATOR: return PlaceGenerator(planePos); break;
            case NodeDTO.BLACK_HOLE : return PlaceBlackHole(planePos); break;
            case NodeDTO.BLOCKADE : return PlaceBlockade(planePos); break;
        }
        return null;
    }
    private BlackHole PlaceBlackHole(Vector2 planePos)
    {
        GameObject obj;
        Vector3 localPos = ToPlaneLocal(planePos);
        if (!IsWithinBounds(localPos) || IsPlaceOccupied(localPos)) return null;
        obj = Instantiate(blackHolePrefab, nodeContainer);
        obj.transform.localPosition = localPos;
        BlackHole blackHole = obj.GetComponent<BlackHole>();
        nodes.Add(blackHole);
        return blackHole;
    }
    private Blockade PlaceBlockade(Vector2 planePos)
    {
        GameObject obj;
        Vector3 localPos = ToPlaneLocal(planePos);
        if (!IsWithinBounds(localPos) || IsPlaceOccupied(localPos)) return null;
        obj = Instantiate(blockadePrefab, nodeContainer);
        obj.transform.localPosition = localPos;
        Blockade blockade = obj.GetComponent<Blockade>();
        nodes.Add(blockade);
        return blockade;
    }
    private Generator PlaceGenerator(Vector2 planePos)
    {
        GameObject obj;
        Vector3 localPos = ToPlaneLocal(planePos);

        if (!IsWithinBounds(localPos) || IsPlaceOccupied(localPos)) return null;
        obj = Instantiate(generatorPrefab, nodeContainer);
        obj.transform.localPosition = localPos;
        Generator generator = obj.GetComponent<Generator>();
        nodes.Add(generator);
        return generator;
    }

    private TimeRipple PlaceTimeRipple(Vector2 planePos, EnergyType energyType)
    {
        GameObject obj;
        Vector3 localPos = ToPlaneLocal(planePos);

        if (!IsWithinBounds(localPos) || IsPlaceOccupied(localPos)) return null;
        obj = Instantiate(nodePrefab, nodeContainer);
        obj.transform.localPosition = localPos;
        TimeRipple timeRipple = obj.GetComponent<TimeRipple>();
        timeRipple.SetEnergyType(energyType);
        nodes.Add(timeRipple);
        return timeRipple;
    }

    public Vector3 SnapToGrid(Vector3 position)
    {
        int x = (int)(position.x / cellWidth);
        int y = (int)(position.y / cellHeight);
        return new Vector3(x, y, 0f) + new Vector3(0.5f, 0.5f, 0f);
    }

    private bool IsWithinBounds(Vector3 position)
    {
        return position.x >= minX && position.x <= maxX && position.y >= minY && position.y <= maxY;
    }

    public NodeVisual IsPlaceOccupied(Vector3 position)
    {
        foreach (NodeVisual node in nodes)
        {
            if ((node.transform.localPosition - position).magnitude < 0.1f)
            {
                return node;
            }
        }

        return null;
    }

    // public Vector3 LocalToWorldPosition(Vector3 localPos)
    // {
    //     Vector3 snappedPos = SnapToGrid(localPos);
    //     return snappedPos;
    // }
    public Vector3 WorldToLocal(Vector3 worldPos)
    {
        Vector3 local = nodeContainer.InverseTransformPoint(worldPos);
        Vector3 scale = frameMesh.localScale;

        return new Vector3(
            local.x + scale.x / 2f,
            local.y + scale.y / 2f,
            0.5f
        );
    }
    
    public Vector3 GridToWorldPosition(Vector2 endPos)
    {
        Vector3 localPos = ToPlaneLocal(endPos);
        return nodeContainer.TransformPoint(localPos);
    }

    public void RemoveNodeVisual(NodeVisual nodeVisual)
    {
        nodes.Remove(nodeVisual);
    }
}