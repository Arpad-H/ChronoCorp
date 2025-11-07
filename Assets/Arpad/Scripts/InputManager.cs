// InputManager.cs
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public GameObject conduitPrefab; // Assign in Inspector
    
    // *** NEW AND IMPORTANT ***
    // Create a Layer in Unity called "Nodes" and assign your NodePrefab to it.
    // Then select that layer here in the Inspector.
    public LayerMask nodeLayerMask; 

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
        tempDrawingLine.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        tempDrawingLine.startColor = Color.yellow;
        tempDrawingLine.endColor = Color.yellow;
        tempDrawingLine.enabled = false;
    }

    void Update()
    {
        if (GameManager.Instance.isGameOver) return;

        // Get mouse position in world space at the current Z level
        float zPos = GameManager.Instance.currentLayerIndex * GameManager.Instance.layerZSpacing;
        mouseWorldPos = GetMouseWorldPosition(zPos);

        // --- While dragging ---
        if (startNode != null)
        {
            // Update the temp line
            tempDrawingLine.SetPosition(1, mouseWorldPos);

            // --- Check for Mouse Button Up (End Drag) ---
            if (Input.GetMouseButtonUp(0))
            {
                // Fire a raycast from the camera to the mouse position
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit; // We use 3D raycast since we are in 3D space

                Node endNode = null;
                
                // Perform the raycast ONLY against the "Nodes" layer
                if (Physics.Raycast(ray, out hit, 100f, nodeLayerMask))
                {
                    // We hit something! Try to get a Node component from it.
                    endNode = hit.collider.GetComponent<Node>();
                }

                // Now, check if we found a valid end node
                if (endNode != null && endNode != startNode)
                {
                    // SUCCESS! Create the conduit.
                    CreateConduit(startNode, endNode);
                }
                
                // No matter what, stop the drag (this clears startNode)
                CancelDrag();
            }
        }
    }

    // Called by Node.cs OnMouseDown()
    public void StartDrag(Node node)
    {
        if (GameManager.Instance.isGameOver || startNode != null) return; // Don't start a new drag if one is active

        startNode = node;
        tempDrawingLine.enabled = true;
        tempDrawingLine.SetPosition(0, startNode.transform.position);
        tempDrawingLine.SetPosition(1, startNode.transform.position);
    }

    // Resets the drag state
    void CancelDrag()
    {
        startNode = null;
        tempDrawingLine.enabled = false;
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