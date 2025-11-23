// InputManager.cs

using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public event Action OnLeftClick;
    public event Action<Vector2> OnMouseMove;
    public event Action<float> OnMouseScroll;

   

    // Temporary line for visual feedback
    private LineRenderer tempDrawingLine;

  void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Create a temporary line renderer for drawing
        // GameObject tempLineObj = new GameObject("TempDrawLine");
        // tempDrawingLine = tempLineObj.AddComponent<LineRenderer>();
        // tempDrawingLine.positionCount = 2;
        // tempDrawingLine.startWidth = 0.1f;
        // tempDrawingLine.endWidth = 0.1f;
        // tempDrawingLine.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        // tempDrawingLine.startColor = Color.yellow;
        // tempDrawingLine.endColor = Color.yellow;
        // tempDrawingLine.enabled = false;
    }

    void Update()
    {
        if (GameStateManager.Instance.isGameOver) return;
        
        // --- On Left Click ---
        if (Input.GetMouseButtonDown(0)) OnLeftClick?.Invoke();
        
        // --- On Mouse Move ---
        OnMouseMove?.Invoke(Input.mousePosition);
        
        // --- On Mouse wheel ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f) OnMouseScroll?.Invoke(scroll);
       
        // --- While dragging ---
        // if (startNode != null)
        // {
        //     // Update the temp line
        //     tempDrawingLine.SetPosition(1, mouseWorldPos);
        //
        //     // --- Check for Mouse Button Up (End Drag) ---
        //     if (Input.GetMouseButtonUp(0))
        //     {
        //         // Fire a raycast from the camera to the mouse position
        //         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //         RaycastHit hit; // We use 3D raycast since we are in 3D space
        //
        //         Node endNode = null;
        //         
        //         // Perform the raycast ONLY against the "Nodes" layer
        //         if (Physics.Raycast(ray, out hit, 100f, nodeLayerMask))
        //         {
        //             // We hit something! Try to get a Node component from it.
        //             endNode = hit.collider.GetComponent<Node>();
        //         }
        //
        //         // Now, check if we found a valid end node
        //         if (endNode != null && endNode != startNode)
        //         {
        //             // SUCCESS! Create the conduit.
        //             CreateConduit(startNode, endNode);
        //         }
        //         
        //         // No matter what, stop the drag (this clears startNode)
        //         CancelDrag();
        //     }
        // }
    }

    // Called by Node.cs OnMouseDown()
    public void StartDrag(Node node)
    {
        // if (GameStateManager.Instance.isGameOver || startNode != null) return; // Don't start a new drag if one is active
        //
        // startNode = node;
        // tempDrawingLine.enabled = true;
        // tempDrawingLine.SetPosition(0, startNode.transform.position);
        // tempDrawingLine.SetPosition(1, startNode.transform.position);
    }

    // Resets the drag state
    void CancelDrag()
    {
        // startNode = null;
        // tempDrawingLine.enabled = false;
    }

    void CreateConduit(Node nodeA, Node nodeB)
    {

    }

    Vector3 GetMouseWorldPosition(float z)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane xyPlane = new Plane(Vector3.forward, new Vector3(0, 0, z));
        xyPlane.Raycast(ray, out float distance);
        return ray.GetPoint(distance);
    }
}