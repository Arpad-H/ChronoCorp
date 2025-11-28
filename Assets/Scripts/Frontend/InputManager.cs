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
            RaycastHit raycastHit;
            Vector3 lineEnd;
            if (cameraController.RaycastForFirst(out raycastHit))
            {
                lineEnd  = raycastHit.point;
            }
            else return;
            
            // Debug: print 3D world position of mouse
            Debug.Log("Mouse World Position: " + lineEnd);
            tempDrawingLine.SetPosition(0, startNode.transform.position);
            tempDrawingLine.SetPosition(1, lineEnd);

            // Check for Mouse Button Up (end drag)
            if (Input.GetMouseButtonUp(0))
            {
                // Raycast against node layer to find end node
                RaycastHit rh;
                if (cameraController.RaycastForFirst(out rh))
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
}