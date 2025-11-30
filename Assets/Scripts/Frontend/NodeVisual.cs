// Node.cs
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class NodeVisual : MonoBehaviour
{
    [Header("Node Properties")]
    public GUID backendID { get; set; }

    public bool isSource; // Is it an energy source or a Time Ripple?
    public float energySupply = 0f; // Energy generated (if source)
    public float energyDemand = 0f; // Energy consumed (if ripple)
    public float nodeScale = 0.75f; // Scale of the node (not overfill the grid)

    [Header("Simulation State")]
    
    public float currentEnergy = 0f; // Current energy buffer
    public float collapseTimer = 0f;
    public const float COLLAPSE_TIME_LIMIT = 5f;

    // References for the simulation
    public List<ConduitVisual> connectedConduits = new List<ConduitVisual>();

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        UpdateColor();
        transform.localScale = Vector3.one * nodeScale;         // Adjust scale to not overcrowd the grid
    }
    

    public void UpdateColor()
    {
        spriteRenderer.color = isSource ? Color.red : Color.cyan;
    }

  private void OnMouseDown()
    {
        InputManager.Instance.StartDrag(this);
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

}