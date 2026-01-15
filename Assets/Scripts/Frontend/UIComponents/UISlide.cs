using UnityEngine;
using UnityEngine.UI;

public class UISlide : MonoBehaviour
{
    public RectTransform panel;
    public float showHeight = 200f;
    public float hiddenHeight = -140f;
    public float buttonHhiddenHeight = -140f;
    public float buttonShowHeight = -470f;
    // public Sprite showIcon;
    // public Sprite hideIcon;
    // public Image buttonIcon;
    public RectTransform buttonPosition;
    public DragItem dragItem; 
    public float speed = 8f;

    bool shown = false;
    Vector2 target;
    Vector2 buttonTarget;

    void Start()
    {
        target = panel.anchoredPosition;
        buttonTarget = buttonPosition.anchoredPosition;
    }

    void Update()
    {
        panel.anchoredPosition = Vector2.Lerp(
            panel.anchoredPosition,
            target,
            Time.deltaTime * speed
        );
        buttonPosition.anchoredPosition = Vector2.Lerp(
            buttonPosition.anchoredPosition,
            buttonTarget,
            Time.deltaTime * speed
        );
    }

    public void Toggle()
    {
        shown = !shown;
        target = shown
            ? new Vector2(panel.anchoredPosition.x,showHeight)
            : new Vector2(panel.anchoredPosition.x,hiddenHeight);
        buttonTarget = shown
            ? new Vector2(buttonPosition.anchoredPosition.x,buttonShowHeight)
            : new Vector2(buttonPosition.anchoredPosition.x,buttonHhiddenHeight);
        // buttonIcon.sprite = shown ? hideIcon : showIcon;
        dragItem.ToggleChanged();
    }
}