using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem current;

    [Header("References")]
    public GameObject tooltipGameObject;
    public TextMeshProUGUI tooltipText;
    public RectTransform tooltipRect;
    
    [Header("Configuration")]
    public Camera uiCamera;   // Drag your UI Camera here!
    public RectTransform safeZone; // Drag your Empty RectTransform here!
    public float padding = 10f; 

    private void Awake()
    {
        current = this;
    }

    public static void Show(string content, RectTransform targetButton)
    {
        current.ShowTooltipInternal(content, targetButton);
    }

    public static void Hide()
    {
        current.tooltipGameObject.SetActive(false);
    }

    private void ShowTooltipInternal(string content, RectTransform targetButton)
    {
        // 1. Initialize
        tooltipText.text = content;
        tooltipGameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);

        // 2. Calculate initial position (Near the button)
        Vector3 worldPos = targetButton.position;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, worldPos);

        // Determine Quadrant (Screen Space)
        bool isRightSide = screenPoint.x > Screen.width * 0.5f;
        bool isTopSide = screenPoint.y > Screen.height * 0.5f;

        // Set Pivot (This makes the tooltip grow away from the mouse)
        tooltipRect.pivot = new Vector2(isRightSide ? 1 : 0, isTopSide ? 1 : 0);

        // Convert Screen Point -> Local Point (Using the Camera!)
        RectTransform tooltipParent = tooltipGameObject.transform.parent as RectTransform;
        Vector2 localPoint;
        
        // IMPORTANT: We pass 'uiCamera' here because you are in Camera Mode
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tooltipParent, 
            screenPoint, 
            uiCamera, 
            out localPoint
        );

        // Apply Offset (Button Size + Padding)
        float xDir = isRightSide ? -1 : 1;
        float yDir = isTopSide ? -1 : 1;
        
        // Use lossyScale to get actual on-screen size
        float btnHalfW = (targetButton.rect.width * targetButton.lossyScale.x) / 2;
        float btnHalfH = (targetButton.rect.height * targetButton.lossyScale.y) / 2;

        localPoint.x += (btnHalfW * xDir) + (padding * xDir);
        localPoint.y += (btnHalfH * yDir) + (padding * yDir);

        // Set the initial guessed position
        tooltipGameObject.transform.localPosition = localPoint;
        
        // Reset Z position to 0 to ensure it doesn't clip behind the camera/canvas
        Vector3 currentLocal = tooltipGameObject.transform.localPosition;
        currentLocal.z = 0; 
        tooltipGameObject.transform.localPosition = currentLocal;

        // 3. The Safe Zone Logic (Push it back onto the screen)
        if (safeZone != null)
        {
            ClampToSafeZone();
        }
    }

    private void ClampToSafeZone()
    {
        // We use World Coordinates for clamping to be safe across different hierarchies
        Vector3[] safeCorners = new Vector3[4];
        safeZone.GetWorldCorners(safeCorners); 
        // 0=BottomLeft, 2=TopRight

        Vector3[] toolCorners = new Vector3[4];
        tooltipRect.GetWorldCorners(toolCorners);

        Vector3 pos = tooltipGameObject.transform.position;

        // Check Left Edge
        if (toolCorners[0].x < safeCorners[0].x)
        {
            float diff = safeCorners[0].x - toolCorners[0].x;
            pos.x += diff;
        }
        // Check Right Edge
        else if (toolCorners[2].x > safeCorners[2].x)
        {
            float diff = toolCorners[2].x - safeCorners[2].x; 
            pos.x -= diff;
        }

        // Check Bottom Edge
        if (toolCorners[0].y < safeCorners[0].y)
        {
            float diff = safeCorners[0].y - toolCorners[0].y;
            pos.y += diff;
        }
        // Check Top Edge
        else if (toolCorners[2].y > safeCorners[2].y)
        {
            float diff = toolCorners[2].y - safeCorners[2].y;
            pos.y -= diff;
        }

        // Apply the corrected position
        tooltipGameObject.transform.position = pos;
    }
}