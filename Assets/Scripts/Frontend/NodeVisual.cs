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
    [Header("Node Properties")] public GUID backendID { get; set; }
    public int layerNum { get; set; }
    public bool isSource; // Is it an energy source or a Time Ripple?
    public EnergyType energyType;

    [Header("Effects")] public GameObject greenGlowEffect;
    public GameObject blueGlowEffect;
    public GameObject redGlowEffect;
    public GameObject yellowGlowEffect;
    public GameObject greenGlowInactiveEffect;
    public GameObject blueGlowInactiveEffect;
    public GameObject redGlowInactiveEffect;
    public GameObject yellowGlowInactiveEffect;
    private GameObject currentGlowEffect;
    public float nodeScale = 0.75f; // Scale of the node (not overfill the grid)
    private bool isBlinking = false;
    private bool blinkToggle;
    [Header("Other")] public SpriteRenderer spriteRenderer;
    public Image hpBar;
    private List<ConduitVisual> connectedConduits = new List<ConduitVisual>(); // References for the simulation
    public Transform attachPoint;
    private bool isEnergySupplied = true;


    void Awake()
    {
        currentGlowEffect = greenGlowEffect;
        UpdateHealthBar(1f);
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        transform.localScale = Vector3.one * nodeScale; // Adjust scale to not overcrowd the grid
        ChangeEnergySupplyState(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        SpawnDeleteButton();
    }

    private void SpawnDeleteButton()
    {
        if (!isSource) return;
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

    public void SetEnergyType(EnergyType newEnergyType)
    {
        // if (!isSource)
        // {
        //     Color color = newEnergyType.ToColor();
        //     energyType = newEnergyType;
        //     spriteRenderer.color = color;
        //     return;
        // }
        energyType = newEnergyType;
        if (!isEnergySupplied)
        {
            ChangeEnergySupplyState(false);
            return;
        }

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
                currentGlowEffect = blueGlowEffect;

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

    public void UpdateHealthBar(float currentValue)
    { 
        if (hpBar) hpBar.fillAmount = currentValue;
        if (currentValue <= BalanceProvider.Balance.noodeBlinkThreshhold && !isEnergySupplied)
        {
            ToggleBlinking(true);
        }
        else
            ToggleBlinking(false);
    }


    private void ToggleBlinking(bool state)
    {
        if (state)
        {
            if (!isBlinking)
            {
                isBlinking = true;
                InvokeRepeating(nameof(BlinkEffect), 0f, 0.5f);
            }
        }
        else
        {
            if (isBlinking)
            {
                isBlinking = false;
                CancelInvoke(nameof(BlinkEffect));
                currentGlowEffect.SetActive(true);
            }
        }
    }

    private void BlinkEffect()
    {
        blinkToggle = !blinkToggle;
        switch (energyType)
        {
            case EnergyType.GREEN:
                greenGlowInactiveEffect.SetActive(!blinkToggle);
                greenGlowEffect.SetActive(blinkToggle);
                break;
            case EnergyType.BLUE:
                blueGlowInactiveEffect.SetActive(!blinkToggle);
                blueGlowEffect.SetActive(blinkToggle);
                break;
            case EnergyType.RED:
                redGlowInactiveEffect.SetActive(!blinkToggle);
                redGlowEffect.SetActive(blinkToggle);
                break;
            case EnergyType.YELLOW:
                yellowGlowInactiveEffect.SetActive(!blinkToggle);
                yellowGlowEffect.SetActive(blinkToggle);
                break;
        }

    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //needed or the OnBeginDrag won't be called
    }

    public void RemoveConnectedConduit(ConduitVisual conduitVisual)
    {
        connectedConduits.Remove(conduitVisual);
        if (connectedConduits.Count == 0)
        {
            ChangeEnergySupplyState(false);
        }
    }

    public void AddConnectedConduit(ConduitVisual conduitVisual)
    {
        connectedConduits.Add(conduitVisual);
        ChangeEnergySupplyState(true);
    }

    private void ChangeEnergySupplyState(bool isSupplied)
    {
        if (isEnergySupplied == isSupplied) return;
        isEnergySupplied = isSupplied;
        if (isEnergySupplied)
        {
            SetEnergyType(energyType);
        }
        else
        {
            switch (energyType)
            {
                case EnergyType.GREEN:
                    currentGlowEffect.SetActive(false);
                    greenGlowInactiveEffect.SetActive(true);
                    currentGlowEffect = greenGlowInactiveEffect;
                    break;
                case EnergyType.BLUE:
                    currentGlowEffect.SetActive(false);
                    blueGlowInactiveEffect.SetActive(true);
                    currentGlowEffect = blueGlowInactiveEffect;
                    break;
                case EnergyType.RED:
                    currentGlowEffect.SetActive(false);
                    redGlowInactiveEffect.SetActive(true);
                    currentGlowEffect = redGlowInactiveEffect;
                    break;
                case EnergyType.YELLOW:
                    currentGlowEffect.SetActive(false);
                    yellowGlowInactiveEffect.SetActive(true);
                    currentGlowEffect = yellowGlowInactiveEffect;
                    break;
            }
        }
    }
}