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
    public Camera uiCamera;

    [Header("Settings")]
    // This now acts as "Padding" away from the button
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
        tooltipText.text = content;
        tooltipGameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);

        // 1. Get the Button's position in Screen Coordinates (Pixels)
        Vector3 worldPos = targetButton.position;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, worldPos);

        // 2. Determine which Quadrant the button is in
        // (0,0 is bottom-left, Screen.width/height is top-right)
        bool isRightSide = screenPoint.x > Screen.width * 0.5f;
        bool isTopSide = screenPoint.y > Screen.height * 0.5f;

        // 3. Set the Pivot dynamically based on the quadrant
        // If on Right side, Pivot X = 1 (Right edge), so it grows Left
        // If on Top side, Pivot Y = 1 (Top edge), so it grows Down
        float pivotX = isRightSide ? 1 : 0;
        float pivotY = isTopSide ? 1 : 0;

        tooltipRect.pivot = new Vector2(pivotX, pivotY);

        // 4. Convert Screen Point to Local Point in the Canvas
        RectTransform tooltipParent = tooltipGameObject.transform.parent as RectTransform;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tooltipParent, 
            screenPoint, 
            uiCamera, 
            out localPoint
        );

        // 5. Calculate the final position at the specific corner of the button
        float buttonHalfWidth = targetButton.rect.width / 2;
        float buttonHalfHeight = targetButton.rect.height / 2;

        // If Right Side: We want to attach to the Left edge of the button (-width)
        // If Left Side: We want to attach to the Right edge of the button (+width)
        float xOffsetDirection = isRightSide ? -1 : 1;
        float yOffsetDirection = isTopSide ? -1 : 1;

        // Apply Button Extents (move to edge of button)
        localPoint.x += (buttonHalfWidth * xOffsetDirection);
        localPoint.y += (buttonHalfHeight * yOffsetDirection);

        // Apply Extra Padding (move slightly further away)
        localPoint.x += (padding * xOffsetDirection);
        localPoint.y += (padding * yOffsetDirection);

        // 6. Apply Final Position
        tooltipGameObject.transform.localPosition = localPoint;
    }
}