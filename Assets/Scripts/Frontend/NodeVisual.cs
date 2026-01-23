// Node.cs

using System;
using UnityEngine;
using System.Collections.Generic;
using Frontend.UIComponents;
using NaughtyAttributes;
using NodeBase;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Util;
using Object = System.Object;

public static class DirectionExtensions
{
    public static Direction GetOpposite(this Direction direction)
    {
        return direction switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }
    public static Vector3 DirectionToVector3(this Direction direction)
    {
        return direction switch
        {
            Direction.Up => new Vector3(0f, 0f, 1f),
            Direction.Down => new Vector3(0f, 0f, -1f),
            Direction.Left =>new Vector3(-1f, 0f, 0f),
            Direction.Right => new Vector3(1f, 0f, 0f),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }
}
public enum  Direction
{
    Up,
    Down,
    Left,
    Right,
}

public class NodeVisual : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler,
    IPointerExitHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IInitializePotentialDragHandler
{
    [Header("Node Properties")] public Guid backendID;
    public int layerNum;
    public float nodeScale = 0.75f; // Scale of the node (not overfill the grid)

    [Header("Other")]
    protected List<ConduitVisual> connectedConduits = new List<ConduitVisual>(); // References for the simulation

    [Header("Attach Points")]
    [Label("Up Direction")] public Transform upPoint;
    [Label("Down Direction")] public Transform downPoint;
    [Label("Left Direction")] public Transform leftPoint;
    [Label("Right Direction")] public Transform rightPoint;
    protected Dictionary<Direction, Transform> attachPoints;
    protected Dictionary<Direction, ConduitVisual> isDirectionOccupied;
    
   
    protected virtual void Awake()
    {
        attachPoints = new Dictionary<Direction, Transform>
        {
            {Direction.Up, upPoint},
            {Direction.Down, downPoint},
            {Direction.Left, leftPoint},
            {Direction.Right, rightPoint}
        };
        isDirectionOccupied = new Dictionary<Direction, ConduitVisual>
        {
            {Direction.Up, null},
            {Direction.Down, null},
            {Direction.Left, null},
            {Direction.Right, null}
        };
        transform.localScale = Vector3.one * nodeScale; // Adjust scale to not overcrowd the grid
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        HandlePointerClick();
    }

    protected virtual void HandlePointerClick()
    {
      
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        ConduitVisualizer.Instance.StartDrag(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        ConduitVisualizer.Instance.CancelDrag();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = Vector3.one * nodeScale * 1.2f;
        ShowInfoWindow(true);
    }

    protected virtual void ShowInfoWindow(bool b)
    {
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one * nodeScale;
        ShowInfoWindow(false);
    }

    public Vector3 GetAttachPosition(Direction direction)
    {
        return attachPoints[direction].position;
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //needed or the OnBeginDrag won't be called
    }

    public virtual void RemoveConnectedConduit(ConduitVisual conduitVisual)
    {
    }

    public virtual void AddConnectedConduit(ConduitVisual conduitVisual,Direction dir)
    {
        
    }
}