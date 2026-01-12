// Node.cs

using System;
using UnityEngine;
using System.Collections.Generic;
using Frontend.UIComponents;
using NodeBase;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Util;
using Object = System.Object;

public class NodeVisual : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler,
    IPointerExitHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IInitializePotentialDragHandler
{
    [Header("Node Properties")]
    public GUID backendID;
    public int layerNum;
    public float nodeScale = 0.75f; // Scale of the node (not overfill the grid)
    
    [Header("Other")]
    protected List<ConduitVisual> connectedConduits = new List<ConduitVisual>(); // References for the simulation
    public Transform attachPoint;
    
    void Start()
    {
        transform.localScale = Vector3.one * nodeScale; // Adjust scale to not overcrowd the grid
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        SpawnDeleteButton();
    }

    private void SpawnDeleteButton()
    {
        DeleteButton deleteBtn = UIManager.Instance.SpawnDeleteButton(transform.position + Vector3.up);
        deleteBtn.Init(() =>
        {
            if (GameFrontendManager.Instance.DestroyNode(backendID))
            {
                foreach (ConduitVisual conduit in new List<ConduitVisual>(connectedConduits))
                {
                    conduit.ConnectedNodeDestroyedConnection(this);
                }

                Destroy(this.gameObject);
                Destroy(deleteBtn.gameObject);
            }
        });
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ConduitVisualizer.Instance.StartDrag(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ConduitVisualizer.Instance.CancelDrag();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = Vector3.one * nodeScale * 1.4f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one * nodeScale;
    }

    public Vector3 GetAttachPosition()
    {
        return attachPoint.position;
    }
    
    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //needed or the OnBeginDrag won't be called
    }

    public virtual void RemoveConnectedConduit(ConduitVisual conduitVisual) { }
    public virtual void AddConnectedConduit(ConduitVisual conduitVisual){}
}