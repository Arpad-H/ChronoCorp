using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class CoordinatePlane : MonoBehaviour
{
    public Transform frameMesh;      // scaled mesh
    public Transform nodeContainer;  // unscaled coordinate space
    
    [Header("Grid Settings")] 
    // public int gridWidth = 10;
    // public int gridHeight = 10; //currently determined by frameMesh scale
    public int cellWidth = 1;
    public int cellHeight = 1;
    
    private List<GameObject> nodes = new List<GameObject>();
    
    void Awake()
    {
        if (!frameMesh) frameMesh = transform.Find("FrameMesh");
        if (!nodeContainer) nodeContainer = transform.Find("NodeContainer");
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

    /// <summary>
    /// Instantiates a node inside the nodeContainer at plane coordinates.
    /// </summary>
    public bool PlaceNode(GameObject prefab, Vector3 planePos)
    {
        Vector3 localPos = ToPlaneLocal(planePos);
        
        if (IsPlaceOccupied(localPos)) return false;
        
        Debug.Log(planePos);
        Debug.Log(localPos);
        GameObject obj = Instantiate(prefab, nodeContainer);
        obj.transform.localPosition = localPos;
        nodes.Add(obj);
        return true;
    }
    private Vector3 SnapToGrid(Vector3 position)
    {
        int x = (int)(position.x / cellWidth) * cellWidth;
        int y = (int)(position.y / cellHeight) * cellHeight;
        return new Vector3(x, y, 0f) + new Vector3(0.5f, 0.5f, 0f);
    }

    private bool IsPlaceOccupied(Vector3 position)
    {
        foreach (GameObject node in nodes) 
        {
            if ((node.transform.localPosition - position).magnitude < 0.1f)
            {
                return true;
            }
        }
        return false;
    }
    
    // public Vector3 LocalToWorldPosition(Vector3 localPos)
    // {
    //     Vector3 snappedPos = SnapToGrid(localPos);
    //     return snappedPos;
    // }
    public Vector3 WorldToLocal(Vector3 worldPos)
    {
        Vector3 local =  nodeContainer.InverseTransformPoint(worldPos);
        Vector3 scale = frameMesh.localScale;
        
        return new Vector3(
            local.x + scale.x / 2f,
            local.y + scale.y / 2f, 
            0.51f
        );
    }
        
    
}