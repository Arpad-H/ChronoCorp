using UnityEngine;
using UnityEngine.UI;

public class UISlide : MonoBehaviour
{
    public RectTransform panel;
    public float showHeight = 200f;
    public float hiddenHeight = -140f;
    public Sprite showIcon;
    public Sprite hideIcon;
    public Image buttonIcon;
    public DragItem dragItem; 
    public float speed = 8f;

    bool shown = false;
    Vector2 target;

    void Start()
    {
        target = panel.anchoredPosition;
    }

    void Update()
    {
        panel.anchoredPosition = Vector2.Lerp(
            panel.anchoredPosition,
            target,
            Time.deltaTime * speed
        );
    }

    public void Toggle()
    {
        shown = !shown;
        target = shown
            ? new Vector2(showHeight,0)
            : new Vector2(hiddenHeight,0);
        buttonIcon.sprite = shown ? hideIcon : showIcon;
        dragItem.ToggleChanged();
    }
}