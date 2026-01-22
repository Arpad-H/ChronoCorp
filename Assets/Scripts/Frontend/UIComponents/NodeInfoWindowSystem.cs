using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NodeInfoWindowSystem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject localTooltipObject; 

    public void OnPointerEnter(PointerEventData eventData)
    {
        localTooltipObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        localTooltipObject.SetActive(false);
    }
}