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
    public event Action OnButtonX;
    public event Action OnButton1;
    public event Action OnButton2;
    public event Action OnButton3;
    public event Action OnButton4;
    public event Action OnButton5;
    public event Action OnButton6;
    public event Action OnButtonD;
    
    
    public event Action<Vector2> OnMouseMove;
    public event Action<float> OnMouseScroll;

    [SerializeField] private LayerMask nodeLayerMask = ~0;
    private NodeVisual _startNodeVisual = null;
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
        
        // --- Button X ---
        if (Input.GetKeyDown(KeyCode.X)) OnButtonX?.Invoke();
        
        // --- Button D ---
        if (Input.GetKeyDown(KeyCode.D)) OnButtonD?.Invoke();
        
        // --- Buttons 1-6 ---
        if (Input.GetKeyDown(KeyCode.Alpha1)) OnButton1?.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha2)) OnButton2?.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha3)) OnButton3?.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha4)) OnButton4?.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha5)) OnButton5?.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha6)) OnButton6?.Invoke();

        // --- While dragging: update temp line and check for mouse up to finish ---
        if (_startNodeVisual != null)
        {
            // Update the line to follow mouse
            RaycastHit raycastHit;
            Vector3 lineEnd;
            if (cameraController.RaycastForFirst(out raycastHit))
            {
                lineEnd  = raycastHit.point;
            }
            else return;
            
           
            tempDrawingLine.SetPosition(0, _startNodeVisual.transform.position);
            tempDrawingLine.SetPosition(1, lineEnd);

            // Check for Mouse Button Up (end drag)
            if (Input.GetMouseButtonUp(0))
            {
                // Raycast against node layer to find end node
                RaycastHit rh;
                if (cameraController.RaycastForFirst(out rh))
                {
                    NodeVisual endNodeVisual = rh.collider.GetComponent<NodeVisual>();
                    if (endNodeVisual != null && endNodeVisual != _startNodeVisual)
                    {
                        // Ask GameStateManager to spawn the conduit between the nodes
                        GameFrontendManager.Instance.SpawnConduit(_startNodeVisual, endNodeVisual);
                    }
                }

                // Cancel drag regardless of result
                CancelDrag();
            }
        }
    }
   
    public void StartDrag(NodeVisual nodeVisual)
    {
        if (_startNodeVisual != null) return; // Don't start a new drag if one is active

        _startNodeVisual = nodeVisual;
        if (tempDrawingLine != null)
        {
            tempDrawingLine.enabled = true;
            tempDrawingLine.SetPosition(0, _startNodeVisual.transform.position);
            tempDrawingLine.SetPosition(1, _startNodeVisual.transform.position);
        }
    }

    // Resets the drag state
    void CancelDrag()
    {
        _startNodeVisual = null;
        if (tempDrawingLine != null) tempDrawingLine.enabled = false;
    }
}