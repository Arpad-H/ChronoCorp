using UnityEngine;

public class ScreenEdgeIcon : MonoBehaviour
{
    public GameObject iconVisual;
    public Transform target;
    public Camera cam;
    public float edgePadding = 30f;

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (!cam) cam = Camera.main;
    }

    void LateUpdate()
    {
        if (!target) return;

        Vector3 screenPos = cam.WorldToScreenPoint(target.position);

        bool behindCamera = screenPos.z < 0f;
        if (behindCamera)
            screenPos *= -1f;

        rectTransform.position = ClampToScreen(screenPos);

        bool onScreen =
            screenPos.z > 0 &&
            screenPos.x > 0 && screenPos.x < Screen.width &&
            screenPos.y > 0 && screenPos.y < Screen.height;

        iconVisual.SetActive(!onScreen);
    }

    Vector2 ClampToScreen(Vector3 screenPos)
    {
        float minX = edgePadding;
        float maxX = Screen.width - edgePadding;
        float minY = edgePadding;
        float maxY = Screen.height - edgePadding;

        return new Vector2(
            Mathf.Clamp(screenPos.x, minX, maxX),
            Mathf.Clamp(screenPos.y, minY, maxY)
        );
    }
}