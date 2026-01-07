// Node.cs

using System;
using UnityEngine;
using System.Collections.Generic;
using NodeBase;
using UnityEditor;
using UnityEngine.UI;

public class NodeVisual : MonoBehaviour
{
    [Header("Node Properties")] 
    public GUID backendID { get; set; }
    public int layerNum { get; set; }
    public String DebugbackendID;
    public bool isSource; // Is it an energy source or a Time Ripple?
    public EnergyType energyType;
    
    [Header("Effects")] 
    public GameObject greenGlowEffect;
    public GameObject blueGlowEffect;
    public GameObject redGlowEffect;
    public GameObject yellowGlowEffect;
    private GameObject currentGlowEffect ;
    public float nodeScale = 0.75f; // Scale of the node (not overfill the grid)
    
    [Header("Other")] 
    public SpriteRenderer spriteRenderer;
    public Image hpBar;
    public List<ConduitVisual> connectedConduits = new List<ConduitVisual>(); // References for the simulation
    public Transform attachPoint;

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
        return attachPoint.position ;
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