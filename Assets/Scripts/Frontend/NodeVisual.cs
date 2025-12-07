// Node.cs

using System;
using UnityEngine;
using System.Collections.Generic;
using NodeBase;
using UnityEditor;

public class NodeVisual : MonoBehaviour
{
    [Header("Node Properties")]
    public GUID backendID { get; set; }

    public String DebugbackendID;

    public EnergyType energyType;
    public bool isSource; // Is it an energy source or a Time Ripple?
    public float energySupply = 0f; // Energy generated (if source)
    public float energyDemand = 0f; // Energy consumed (if ripple)
    public float nodeScale = 0.75f; // Scale of the node (not overfill the grid)
    public SpriteRenderer spriteRenderer;
    
    
    public float currentEnergy = 0f; // Current energy buffer
    public float collapseTimer = 0f;
    public const float COLLAPSE_TIME_LIMIT = 5f;

    // References for the simulation
    public List<ConduitVisual> connectedConduits = new List<ConduitVisual>();

   

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        transform.localScale = Vector3.one * nodeScale;         // Adjust scale to not overcrowd the grid
    }
    

  private void OnMouseDown()
    {
        ConduitVisualizer.Instance.StartDrag(this);
    }

    private void OnMouseEnter()
    {
        transform.localScale = Vector3.one * nodeScale * 1.4f; // Highlight
    }

    private void OnMouseExit()
    {
        transform.localScale = Vector3.one * nodeScale;
    }

    public Vector3 GetAttachPosition()
    {
        return transform.position;
    }
    public void SetEnergyType(EnergyType newEnergyType)
    {
        Color color = newEnergyType.ToColor();
        energyType = newEnergyType;
        spriteRenderer.color = color;
    }

    private void Update()
    {
        DebugbackendID = backendID.ToString();
    }
}