using System.Collections;
using Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragableInventoyItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool isDraggable = true;
    public Sprite iconImage; // assign from inspector
    public Canvas canvas; // drag your UI canvas here
    private Image dragIcon;
    private RectTransform dragRect;
    public int count = 0;
    public TextMeshProUGUI countText;
    public InventoryItem item;
    public Image InventoryIcon;
    public UISlide uiSlide;
    void Start()
    {
       
        GameFrontendManager.Instance.GeneratorDeleted += UpdateCountText;
        GameFrontendManager.Instance.InventoryChanged += UpdateCountText;
        InputManager.Instance.OnRightClick += CancelDrag;
        StartCoroutine(DelayedUpdate());
    }
    IEnumerator DelayedUpdate()
    {
        yield return new WaitForEndOfFrame();
        UpdateCountText();
    }

    private void UpdateCountText()
    {
        count = GameFrontendManager.Instance.GetInvetoryCount(item);
        countText.text = count.ToString();
        if (count <= 0)
        {
            InventoryIcon.color = new Color(1f, 0, 0, 1); 
        }
        else
        {
            InventoryIcon.color = new Color(1f, 1f, 1f, 1f); 
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable || count <= 0) return;
        //if (uiSlide) uiSlide.Toggle();
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
        if (!isDraggable || count <= 0) return;
        if (dragRect != null)
            dragRect.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable || count <= 0) return;
        if (dragIcon != null)
            Destroy(dragIcon.gameObject);
        else return;
        if (GameFrontendManager.Instance.TryDrop(item))
        {
            UpdateCountText();
        }
    }
    public void CancelDrag() 
    {
        if (dragIcon != null)
            Destroy(dragIcon.gameObject);
    }
}