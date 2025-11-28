using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class CoordinatePlane : MonoBehaviour
{
    public Transform frameMesh; // scaled mesh
    public Transform nodeContainer; // unscaled coordinate space
    public Material mGrid;
    public Material mNormal;
    [Header("Grid Settings")]
    // public int gridWidth = 10;
    // public int gridHeight = 10; //currently determined by frameMesh scale
    public int cellWidth = 1;
    public int cellHeight = 1;
    public bool showGrid = false;
    private int numX, numY; // Number of cells in each direction
    private float maxX, maxY, minX, minY; // Max/min bounds (avoid placing outside frame)

    private List<GameObject> nodes = new List<GameObject>();

    void Awake()
    {
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

    void Update()
    {
        if (showGrid) frameMesh.GetComponent<MeshRenderer>().material = mGrid;
        else frameMesh.GetComponent<MeshRenderer>().material = mNormal;
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
    public bool PlaceNode(GameObject prefab, Vector3 planePos, out GameObject obj)
    {
        Vector3 localPos = ToPlaneLocal(planePos);
        obj = null;
        if (!IsWithinBounds(localPos) || IsPlaceOccupied(localPos)) return false;

        obj = Instantiate(prefab, nodeContainer);
        obj.transform.localPosition = localPos;
        nodes.Add(obj);
        return true;
    }

    private Vector3 SnapToGrid(Vector3 position)
    {
        int x = (int)(position.x / cellWidth);
        int y = (int)(position.y / cellHeight);
        Debug.Log($"Snapped to grid: ({x}, {y})");
        return new Vector3(x, y, 0f) + new Vector3(0.5f, 0.5f, 0f);
    }

    private bool IsWithinBounds(Vector3 position)
    {
        return position.x >= minX && position.x <= maxX && position.y >= minY && position.y <= maxY;
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
        Vector3 local = nodeContainer.InverseTransformPoint(worldPos);
        Vector3 scale = frameMesh.localScale;

        return new Vector3(
            local.x + scale.x / 2f,
            local.y + scale.y / 2f,
            0.5f
        );
    }

    // private void DrawGridLines()
    // {
    //     if (frameMesh == null) return;
    //
    //     GameObject gridParent = new GameObject("DebugGrid");
    //     gridParent.transform.parent = nodeContainer;
    //     gridParent.transform.localPosition = Vector3.zero;
    //     float zOffset = -0.51f;
    //
    //     // Vertical lines
    //     for (int x = 0; x <= numX; x++)
    //     {
    //         GameObject lineObj = new GameObject("VLine_" + x);
    //         lineObj.transform.parent = gridParent.transform;
    //         LineRenderer lr = lineObj.AddComponent<LineRenderer>();
    //         lr.positionCount = 2;
    //         lr.startWidth = 0.04f;
    //         lr.endWidth = 0.04f;
    //         lr.useWorldSpace = false;
    //         lr.material = new Material(Shader.Find("Sprites/Default"));
    //         lr.startColor = lr.endColor = Color.black;
    //
    //         Vector3 start = new Vector3(minX + x * cellWidth, minY, zOffset);
    //         Vector3 end = new Vector3(minX + x * cellWidth, maxY, zOffset);
    //         lr.SetPosition(0, start);
    //         lr.SetPosition(1, end);
    //     }
    //
    //     // Horizontal lines
    //     for (int y = 0; y <= numY; y++)
    //     {
    //         GameObject lineObj = new GameObject("HLine_" + y);
    //         lineObj.transform.parent = gridParent.transform;
    //         LineRenderer lr = lineObj.AddComponent<LineRenderer>();
    //         lr.positionCount = 2;
    //         lr.startWidth = 0.04f;
    //         lr.endWidth = 0.04f;
    //         lr.useWorldSpace = false;
    //         lr.material = new Material(Shader.Find("Sprites/Default"));
    //         lr.startColor = lr.endColor = Color.black;
    //
    //         Vector3 start = new Vector3(minX, minY + y * cellHeight, zOffset);
    //         Vector3 end = new Vector3(maxX, minY + y * cellHeight, zOffset);
    //         lr.SetPosition(0, start);
    //         lr.SetPosition(1, end);
    //     }
    // }
}