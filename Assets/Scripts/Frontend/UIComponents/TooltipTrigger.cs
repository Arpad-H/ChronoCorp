using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections; // Required for Coroutines

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea]
    public string content;

    [Tooltip("How long to wait before showing the tooltip (seconds)")]
    public float delay = 0.5f; // 0.5 seconds is standard

    private RectTransform _rectTransform;
    private Coroutine _delayCoroutine;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Start the timer
        _delayCoroutine = StartCoroutine(ShowAfterDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 1. If the timer is still running, stop it! (Prevent the show)
        if (_delayCoroutine != null)
        {
            StopCoroutine(_delayCoroutine);
            _delayCoroutine = null;
        }

        // 2. Hide the tooltip immediately
        TooltipSystem.Hide();
    }

    private IEnumerator ShowAfterDelay()
    {
        // Wait for the delay
        yield return new WaitForSeconds(delay);

        // Show the tooltip
        TooltipSystem.Show(content, _rectTransform);
    }
    
    // Safety check: If the object is disabled while hovering, stop the timer
    private void OnDisable()
    {
        TooltipSystem.Hide();
    }
}