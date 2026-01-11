using Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Sprite iconImage; // assign from inspector
    public Canvas canvas; // drag your UI canvas here
    private Image dragIcon;
    private RectTransform dragRect;
    public int count = 3;
    public TextMeshProUGUI countText;
    public NodeDTO item;

    void Start()
    {
        UpdateCountText();
       
    }

    private void UpdateCountText()
    {
        count = GameFrontendManager.Instance.GetInvetoryCount(item);
        countText.text = count.ToString();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragIcon = new GameObject("DragIcon").AddComponent<Image>();
        dragIcon.sprite = iconImage;
        dragIcon.raycastTarget = false;
        dragRect = dragIcon.GetComponent<RectTransform>();
        dragRect.SetParent(canvas.transform, false);
        dragRect.position = eventData.position;
        dragRect.localScale = Vector3.one;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragRect != null)
            dragRect.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
            Destroy(dragIcon.gameObject);
        if (GameFrontendManager.Instance.TryDrop(NodeDTO.GENERATOR))
        {
            count--;
            UpdateCountText();
        }
    }
}