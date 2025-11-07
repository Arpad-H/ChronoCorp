// Node.cs
using UnityEngine;
using System.Collections.Generic;

public class Node : MonoBehaviour
{
    [Header("Node Properties")]
    public int id;
    public bool isSource; 
    public float energySupply = 0f;
    public float energyDemand = 0f; 

    [Header("Simulation State")]
    public float currentEnergy = 0f; // Current energy buffer
    public float collapseTimer = 0f;
    public const float COLLAPSE_TIME_LIMIT = 5f;

    // References for the simulation
    public List<Conduit> connectedConduits = new List<Conduit>();

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Assign a unique ID. In a real game, this would be more robust.
        id = GetInstanceID(); 
    }

    void Start()
    {
        UpdateColor();
    }

    // This simulation step is for the PRESENT layer
    public void SimulateStep(float networkEfficiency)
    {
        if (isSource)
        {
            currentEnergy = energySupply;
        }
        else
        {
            // If the network is failing, this ripple only gets a fraction of its demand
            float energyReceived = energyDemand * networkEfficiency;
            currentEnergy = energyReceived;

        
            if (currentEnergy < energyDemand * 0.99f) // Check if energy is insufficient
            {
                collapseTimer += Time.deltaTime;
                spriteRenderer.color = Color.red; // Visual feedback
                if (collapseTimer > COLLAPSE_TIME_LIMIT)
                {
                   GameManager.Instance.GameOver("A Time Ripple collapsed!"); 
                }
            }
            else
            {
                collapseTimer = 0f;
                UpdateColor();
            }
        }
    }

    public void UpdateColor()
    {
        spriteRenderer.color = isSource ? Color.green : Color.cyan;
    }

    // --- Mouse Interaction Callbacks ---
    // These are used by InputManager to start/end conduit connections
    private void OnMouseDown()
    {
        InputManager.Instance.OnNodeClicked(this);
    }

    private void OnMouseUp()
    {
        InputManager.Instance.OnNodeClicked(this);
    }

    private void OnMouseEnter()
    {
        transform.localScale = Vector3.one * 1.2f; // Highlight
    }

    private void OnMouseExit()
    {
        transform.localScale = Vector3.one;
    }
}