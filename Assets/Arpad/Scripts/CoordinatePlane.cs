using UnityEngine;

public class CoordinatePlane : MonoBehaviour
{
    public Transform frameMesh;      // scaled mesh
    public Transform nodeContainer;  // unscaled coordinate space
    
    [Header("Grid Settings")] 
    public int gridWidth = 10;
    public int gridHeight = 10;
    public int cellWidth = 1;
    public int cellHeight = 1;
    
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
    public Transform PlaceNode(GameObject prefab, Vector3 planePos)
    {
      
        Vector3 localPos = ToPlaneLocal(planePos);
        GameObject obj = Instantiate(prefab, nodeContainer);
        obj.transform.localPosition = localPos;
        return obj.transform;
    }
    private Vector3 SnapToGrid(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / cellWidth) * cellWidth;
        int y = Mathf.RoundToInt(position.y / cellHeight) * cellHeight;
        return new Vector3(x, y, 0f) + new Vector3(0.5f, 0.5f, 0f);
    }
    
    // public Vector3 LocalToWorldPosition(Vector3 localPos)
    // {
    //     Vector3 snappedPos = SnapToGrid(localPos);
    //     return snappedPos;
    // }
    public Vector3 WorldToLocal(Vector3 worldPos)
    {
        return nodeContainer.InverseTransformPoint(worldPos);
    }
        
    
}