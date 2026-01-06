// Node.cs

using System;
using UnityEngine;
using System.Collections.Generic;
using NodeBase;
using UnityEditor;
using UnityEngine.UI;

public class NodeVisual : MonoBehaviour
{
    [Header("Node Properties")] public GUID backendID { get; set; }

    public String DebugbackendID;

    public GameObject greenGlowEffect;
    public GameObject blueGlowEffect;
    public GameObject redGlowEffect;
    public GameObject yellowGlowEffect;
    private GameObject currentGlowEffect ;
    

    public EnergyType energyType;
    public bool isSource; // Is it an energy source or a Time Ripple?
    public float energySupply = 0f; // Energy generated (if source)
    public float energyDemand = 0f; // Energy consumed (if ripple)
    public float nodeScale = 0.75f; // Scale of the node (not overfill the grid)
    public SpriteRenderer spriteRenderer;
    public Image hpBar;

    public float currentEnergy = 0f; // Current energy buffer
    public float collapseTimer = 0f;
    public const float COLLAPSE_TIME_LIMIT = 5f;

    // References for the simulation
    public List<ConduitVisual> connectedConduits = new List<ConduitVisual>();


    void Awake()
    {
        currentGlowEffect = greenGlowEffect;
       UpdateHealthBar(1f);
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        transform.localScale = Vector3.one * nodeScale; // Adjust scale to not overcrowd the grid
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
        // if (!isSource)
        // {
        //     Color color = newEnergyType.ToColor();
        //     energyType = newEnergyType;
        //     spriteRenderer.color = color;
        //     return;
        // }


        switch (newEnergyType)
        {
            case EnergyType.GREEN:
                greenGlowEffect.SetActive(true);
                currentGlowEffect.SetActive(false);
                currentGlowEffect = greenGlowEffect;

                break;
            case EnergyType.BLUE:
                blueGlowEffect.SetActive(true);
                currentGlowEffect.SetActive(false);
                currentGlowEffect = greenGlowEffect;
                break;
            case EnergyType.RED:
                redGlowEffect.SetActive(true);
                currentGlowEffect.SetActive(false);
                currentGlowEffect = redGlowEffect;
                break;
            case EnergyType.YELLOW:
                yellowGlowEffect.SetActive(true);
                currentGlowEffect.SetActive(false);
                currentGlowEffect = yellowGlowEffect;
                break;
        }
    }

    private void Update()
    {
        DebugbackendID = backendID.ToString();
    }

    public void UpdateHealthBar(float currentValue)
    {
        if (hpBar)hpBar.fillAmount = currentValue;
    }
}