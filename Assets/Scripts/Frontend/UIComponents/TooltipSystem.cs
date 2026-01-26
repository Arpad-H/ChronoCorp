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
       
        tooltipText.text = content;
        tooltipGameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);

    
        Vector3 worldPos = targetButton.position;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, worldPos);

       
        bool isRightSide = screenPoint.x > Screen.width * 0.5f;
        bool isTopSide = screenPoint.y > Screen.height * 0.5f;

      
        tooltipRect.pivot = new Vector2(isRightSide ? 1 : 0, isTopSide ? 1 : 0);

        
        RectTransform tooltipParent = tooltipGameObject.transform.parent as RectTransform;
        Vector2 localPoint;
        
      
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tooltipParent, 
            screenPoint, 
            uiCamera, 
            out localPoint
        );

     
        float xDir = isRightSide ? -1 : 1;
        float yDir = isTopSide ? -1 : 1;
        
      
        float btnHalfW = (targetButton.rect.width * targetButton.lossyScale.x) / 2;
        float btnHalfH = (targetButton.rect.height * targetButton.lossyScale.y) / 2;

        localPoint.x += (btnHalfW * xDir) + (padding * xDir);
        localPoint.y += (btnHalfH * yDir) + (padding * yDir);

      
        tooltipGameObject.transform.localPosition = localPoint;
        
      
        Vector3 currentLocal = tooltipGameObject.transform.localPosition;
        currentLocal.z = 0; 
        tooltipGameObject.transform.localPosition = currentLocal;

       
        if (safeZone != null)
        {
            tooltipGameObject.SetActive(true);
            tooltipText.text = content;


            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
            Canvas.ForceUpdateCanvases();
            ClampToSafeZone();
        }
    }

    private void ClampToSafeZone()
    {
        if (safeZone == null) return;

        // Clamp in the tooltip parent's local space (same space you place it in).
        RectTransform root = tooltipRect.parent as RectTransform;
        if (root == null) return;

        // Bounds of safeZone and tooltipRect, both expressed in 'root' local space.
        Bounds safeB = RectTransformUtility.CalculateRelativeRectTransformBounds(root, safeZone);
        Bounds tipB  = RectTransformUtility.CalculateRelativeRectTransformBounds(root, tooltipRect);

        Vector3 delta = Vector3.zero;

        // X clamp
        if (tipB.min.x < safeB.min.x)
            delta.x += (safeB.min.x - tipB.min.x);
        else if (tipB.max.x > safeB.max.x)
            delta.x -= (tipB.max.x - safeB.max.x);

        // Y clamp
        if (tipB.min.y < safeB.min.y)
            delta.y += (safeB.min.y - tipB.min.y);
        else if (tipB.max.y > safeB.max.y)
            delta.y -= (tipB.max.y - safeB.max.y);

        // Apply in the same space we measured in.
        tooltipRect.localPosition += delta;

        // Keep z clean if you want
        var p = tooltipRect.localPosition;
        p.z = 0;
        tooltipRect.localPosition = p;
    }
}