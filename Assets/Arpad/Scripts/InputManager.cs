// InputManager.cs
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public GameObject conduitPrefab; // Assign in Inspector
    public LayerMask nodeLayer; // Set this later

    private Node startNode;
    private Vector3 mouseWorldPos;

    // Temporary line for visual feedback
    private LineRenderer tempDrawingLine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Create a temporary line renderer for drawing
        GameObject tempLineObj = new GameObject("TempDrawLine");
        tempDrawingLine = tempLineObj.AddComponent<LineRenderer>();
        tempDrawingLine.positionCount = 2;
        tempDrawingLine.startWidth = 0.1f;
        tempDrawingLine.endWidth = 0.1f;
        tempDrawingLine.material.color = Color.yellow; // Make it obvious
        tempDrawingLine.enabled = false;
    }

    void Update()
    {
        if (GameManager.Instance.isGameOver) return;

        // Get mouse position in world space at the current Z level
        float zPos = GameManager.Instance.currentLayerIndex * GameManager.Instance.layerZSpacing;
        mouseWorldPos = GetMouseWorldPosition(zPos);

        // While drawing a line
        if (startNode != null && Input.GetMouseButton(0))
        {
            tempDrawingLine.SetPosition(1, mouseWorldPos);
        }

        // --- Mouse Button Up (Cancel or Finish) ---
        if (Input.GetMouseButtonUp(0))
        {
            startNode = null;
            tempDrawingLine.enabled = false;
        }
    }

    // This is called by Node.cs via OnMouseDown/OnMouseUp
    public void OnNodeClicked(Node clickedNode)
    {
        if (GameManager.Instance.isGameOver) return;

        // This is the click-down event
        if (Input.GetMouseButtonDown(0))
        {
            startNode = clickedNode;
            tempDrawingLine.enabled = true;
            tempDrawingLine.SetPosition(0, startNode.transform.position);
            tempDrawingLine.SetPosition(1, startNode.transform.position);
        }
        // This is the click-up event
        else if (Input.GetMouseButtonUp(0) && startNode != null)
        {
            // If we release on a *different* node, create a conduit
            if (clickedNode != startNode)
            {
                CreateConduit(startNode, clickedNode);
            }
        }
    }

    void CreateConduit(Node nodeA, Node nodeB)
    {
        // Check if a conduit already exists
        foreach (Conduit c in nodeA.connectedConduits)
        {
            if (c.nodeA == nodeB || c.nodeB == nodeB)
                return; // Conduit already exists
        }

        GameObject conduitObj = Instantiate(conduitPrefab, Vector3.zero, Quaternion.identity);
        Conduit newConduit = conduitObj.GetComponent<Conduit>();
        newConduit.Initialize(nodeA, nodeB);
        
        GameManager.Instance.AddConduit(newConduit);
    }

    Vector3 GetMouseWorldPosition(float z)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane xyPlane = new Plane(Vector3.forward, new Vector3(0, 0, z));
        xyPlane.Raycast(ray, out float distance);
        return ray.GetPoint(distance);
    }
}