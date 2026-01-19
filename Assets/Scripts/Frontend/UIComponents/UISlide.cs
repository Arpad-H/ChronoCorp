using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UISlide : MonoBehaviour
{
    public RectTransform windowTransform;
    public float speed = 12f; // Faster speed feels more "snappy"/retro
    
    [Header("Animation Settings")]
    public Vector3 hiddenScale = new Vector3(0.1f, 0.01f, 1f); // Start thin and small
    public Vector3 shownScale = Vector3.one;

    private bool isShown = false;
    private Vector3 targetScale;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        
        // Initialize state
        windowTransform.localScale = hiddenScale;
        targetScale = hiddenScale;
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }

    void Update()
    {
        // Smoothly lerp the scale
        windowTransform.localScale = Vector3.Lerp(
            windowTransform.localScale, 
            targetScale, 
            Time.deltaTime * speed
        );

        // Fade alpha based on scale progress
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, isShown ? 1 : 0, Time.deltaTime * speed);
    }

    public void Toggle()
    {
        isShown = !isShown;
        
        targetScale = isShown ? shownScale : hiddenScale;
        
        // Prevent clicking items when the window is hidden
        canvasGroup.blocksRaycasts = isShown;
        canvasGroup.interactable = isShown;
    }
}