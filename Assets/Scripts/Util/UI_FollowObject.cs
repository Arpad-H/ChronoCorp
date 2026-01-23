using UnityEngine;

public class UI_FollowObjecte : MonoBehaviour
{
    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 2f, 0); // Offset in 3D world units (move it up above the node)

    private Transform targetNode;
    private RectTransform myRectTransform;
    private Canvas parentCanvas;
    private Camera mainCamera;

    void Awake()
    {
        myRectTransform = GetComponent<RectTransform>();
        // Cache the camera. If your camera changes (e.g. cutscenes), do this in Start or Update.
        mainCamera = Camera.main; 
        
        // Find the canvas automatically if not assigned
        parentCanvas = GetComponentInParent<Canvas>();
    }

    // Call this when you show the window
    public void SetTarget(Transform target)
    {
        targetNode = target;
        UpdatePosition(); // Force an update immediately so it doesn't flicker
    }

    // LateUpdate is better for UI following to prevent "jitter" as the camera moves
    void LateUpdate()
    {
        if (targetNode == null || parentCanvas == null) return;

        UpdatePosition();
    }

    void UpdatePosition()
    {
        // 1. Get the 3D position of the node + offset
        Vector3 worldPos = targetNode.position + offset;

        // 2. Convert World 3D -> Screen 2D (Pixels)
        Vector2 screenPoint = mainCamera.WorldToScreenPoint(worldPos);

        // 3. Convert Screen Pixels -> Canvas Local Coordinates
        // This handles difference in resolutions and Canvas Scaler settings perfectly
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform, 
            screenPoint, 
            parentCanvas.worldCamera, 
            out localPoint
        );

        // 4. Apply to the UI
        myRectTransform.anchoredPosition = localPoint;
    }
}