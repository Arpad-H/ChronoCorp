using NodeBase;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Util;

public class TimeRipple : NodeVisual
{
    public EnergyType energyType;
    [Header("Effects")] 
    public GameObject greenGlowEffect;
    public GameObject blueGlowEffect;
    public GameObject redGlowEffect;
    public GameObject yellowGlowEffect;
    public GameObject greenGlowInactiveEffect;
    public GameObject blueGlowInactiveEffect;
    public GameObject redGlowInactiveEffect;
    public GameObject yellowGlowInactiveEffect;
    private GameObject currentGlowEffect;
    private bool isBlinking = false;
    private bool blinkToggle;
    public GameObject ScreenEdgeIconPrefab;
    private ScreenEdgeIcon screenEdgeIcon;
    
    [Header("Other")]
    public Image hpBar;
    private bool isEnergySupplied = true;
    protected override void Awake()
    {
        base.Awake();
        currentGlowEffect = greenGlowEffect;
        UpdateHealthBar(1f);
    }

    void Start()
    {
        ChangeEnergySupplyState(false);
    }

    public new void OnPointerEnter(PointerEventData eventData)
    {
        //because not deletable by player
    }
    
    public void SetEnergyType(EnergyType newEnergyType)
    {
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
    
    public void UpdateHealthBar(float currentValue)
    { 
        hpBar.fillAmount = currentValue;
        if (currentValue <= BalanceProvider.Balance.nodeBlinkThreshhold && !isEnergySupplied)
        {
            if(!screenEdgeIcon)
            {
                screenEdgeIcon = Instantiate(ScreenEdgeIconPrefab).GetComponentInChildren<ScreenEdgeIcon>();
                screenEdgeIcon.target = this.transform;
            }
            ToggleBlinking(true);
        }
        else
        {
            if (screenEdgeIcon) Destroy(screenEdgeIcon);
            ToggleBlinking(false);
        }
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
    
    public override void RemoveConnectedConduit(ConduitVisual conduitVisual)
    {
        foreach (var dir in isDirectionOccupied)
        {
            if (dir.Value == conduitVisual)
            {
                isDirectionOccupied[dir.Key] = null;
                break;
            }
        }
        connectedConduits.Remove(conduitVisual);
        if (connectedConduits.Count == 0)
        {
            ChangeEnergySupplyState(false);
        }
    }
    public override void AddConnectedConduit(ConduitVisual conduitVisual,Direction dir)
    {
        isDirectionOccupied[dir] = conduitVisual;
        connectedConduits.Add(conduitVisual);
        ChangeEnergySupplyState(true);
    }
}
