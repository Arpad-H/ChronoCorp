// InputManager.cs

using System;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public event Action OnLeftClick;
    public event Action OnButtonN;
    public event Action OnButtonG;
    public event Action OnButtonD;
    
    
    public event Action<Vector2> OnMouseMove;
    public event Action<float> OnMouseScroll;

    [SerializeField] private LayerMask nodeLayerMask = ~0;
    private Node startNode = null;
    // Temporary line for visual feedback
    private LineRenderer tempDrawingLine = null;
    [SerializeField] private float tempLineWidth = 0.05f;

    public CameraController cameraController;
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
        if (cameraController == null) cameraController = FindObjectOfType<CameraController>();
        {
            
        }
        // create a temporary line renderer for drawing if not created in inspector
        if (tempDrawingLine == null)
        {
            GameObject tempLineObj = new GameObject("TempDrawLine");
            tempLineObj.transform.SetParent(transform);
            tempDrawingLine = tempLineObj.AddComponent<LineRenderer>();
            tempDrawingLine.positionCount = 2;
            tempDrawingLine.startWidth = tempLineWidth;
            tempDrawingLine.endWidth = tempLineWidth;
            tempDrawingLine.useWorldSpace = true;
            tempDrawingLine.enabled = false;
        }
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
        
        // --- Button N ---
        if (Input.GetKeyDown(KeyCode.N)) OnButtonN?.Invoke();
        
        // --- Button G ---
        if (Input.GetKeyDown(KeyCode.G)) OnButtonG?.Invoke();
        
        // --- Button D ---
        if (Input.GetKeyDown(KeyCode.D)) OnButtonD?.Invoke();

        // --- While dragging: update temp line and check for mouse up to finish ---
        if (startNode != null)
        {
            // Update the line to follow mouse
            // Vector3 lineEnd = GetMouseWorldPosition(startNode.transform.position.z);
            Vector3 lineEnd = cameraController.RaycastAll()[0].point;
            // Debug: print 3D world position of mouse
            Debug.Log("Mouse World Position: " + lineEnd);
            tempDrawingLine.SetPosition(0, startNode.transform.position);
            tempDrawingLine.SetPosition(1, lineEnd);

            // Check for Mouse Button Up (end drag)
            if (Input.GetMouseButtonUp(0))
            {
                // Raycast against node layer to find end node
                RaycastHit rh = cameraController.RaycastAll()[0];
                if (rh.collider != null)
                {
                    Node endNode = rh.collider.GetComponent<Node>();
                    if (endNode != null && endNode != startNode)
                    {
                        // Ask GameStateManager to spawn the conduit between the nodes
                        GameStateManager.Instance.SpawnConduit(startNode, endNode);
                        // Print debug message
                        Debug.Log($"Creating conduit between Node {startNode.id} and Node {endNode.id}");
                    }
                }

                // Cancel drag regardless of result
                CancelDrag();
            }
        }
        // Previous code (remove once new code is confirmed working)
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
   
    public void StartDrag(Node node)
    {
        if (GameStateManager.Instance.isGameOver || startNode != null) return; // Don't start a new drag if one is active

        startNode = node;
        if (tempDrawingLine != null)
        {
            tempDrawingLine.enabled = true;
            tempDrawingLine.SetPosition(0, startNode.transform.position);
            tempDrawingLine.SetPosition(1, startNode.transform.position);
        }
    }

    // Resets the drag state
    void CancelDrag()
    {
        startNode = null;
        if (tempDrawingLine != null) tempDrawingLine.enabled = false;
    }

    Vector3 GetMouseWorldPosition(float z)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane xyPlane = new Plane(Vector3.forward, new Vector3(0, 0, z));
        xyPlane.Raycast(ray, out float distance);
        return ray.GetPoint(distance);
    }

    // Vector3 GetMouseWorldPositionOnHoveredSlice(Vector3 fallbackPosition)
    // {
    //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //     RaycastHit hit;

    //     // Assuming all your slices have the layer "Slices" or a tag "Slice"
    //     int sliceLayerMask = LayerMask.GetMask("Slices"); 

    //     if (Physics.Raycast(ray, out hit, 100f, sliceLayerMask))
    //     {
    //         // We hit a slice — convert world position to local of that slice if needed
    //         return hit.point;
    //     }
    //     else
    //     {
    //         // Nothing hit —> fallback to start node slice
    //         return fallbackPosition;
    //     }
    // }

    Vector3 GetMouseWorldPositionOnPossibleLayers(Vector3 fallbackPosition, int startNodeLayerZ)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f); // all hits
        List<int> validLayerIndices = new List<int>();

        for (int i = 0; i < 3; i++)
        {
            int idx = startNodeLayerZ - i;
            if (idx >= 0 && idx < GameStateManager.Instance.temporalLayers.Count)
                validLayerIndices.Add(idx);
        }

        // foreach (var hit in hits)
        // {
        //     CoordinatePlane plane = hit.transform.GetComponentInParent<CoordinatePlane>();
        //     if (plane != null && validLayerIndices.Contains(plane.layerIndex))
        //     {
        //         return hit.point;   // First valid slice hit
        //     }
        // }

        return fallbackPosition;    // By default use the slice of the start node
    }


}