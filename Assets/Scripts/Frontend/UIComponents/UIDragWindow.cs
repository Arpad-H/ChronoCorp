using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragWindow : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    [SerializeField] private RectTransform windowToMove; // Drag the whole window here
    [SerializeField] private Canvas canvas; // Drag your main Canvas here

    private Vector2 pointerOffset;

    void Awake()
    {
        // If not assigned, assume the parent is the window
        if (windowToMove == null)
            windowToMove = transform.parent.GetComponent<RectTransform>();
            
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Calculate the offset so the window doesn't "snap" its center to the mouse
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            windowToMove, 
            eventData.position, 
            eventData.pressEventCamera, 
            out pointerOffset
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (windowToMove == null || canvas == null) return;

        // Convert mouse position to a position relative to the Canvas
        Vector2 localPointerPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out localPointerPos))
        {
            // Move the window, maintaining the initial click offset
            windowToMove.anchoredPosition = localPointerPos - pointerOffset;
        }
    }
}