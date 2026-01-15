using NodeBase;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Util;

public class TimeRipple : NodeVisual
{
    public EnergyType energyType;
    [Header("Effects")] 
    public Material greenGlowEffect;
    public Material blueGlowEffect;
    public Material redGlowEffect;
    public Material yellowGlowEffect;
    public Material greenGlowInactiveEffect;
    public Material blueGlowInactiveEffect;
    public Material redGlowInactiveEffect;
    public Material yellowGlowInactiveEffect;
    private Material currentGlowEffect;
    private bool isBlinking = false;
    private bool blinkToggle;
    public GameObject ScreenEdgeIconPrefab;
    private ScreenEdgeIcon screenEdgeIcon;
    
    [Header("Other")]
    public Renderer _renderer;
   // public Image hpBar;
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
            currentGlowEffect = greenGlowEffect;
   
                break;
            case EnergyType.BLUE:
                currentGlowEffect = blueGlowEffect;

                break;
            case EnergyType.RED:
                currentGlowEffect = redGlowEffect;

                break;
            case EnergyType.YELLOW:
                currentGlowEffect = yellowGlowEffect;

                break;
        }
        _renderer.material = currentGlowEffect;
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
                    currentGlowEffect = greenGlowInactiveEffect;
                  
                    break;
                case EnergyType.BLUE:
                   
                    currentGlowEffect = blueGlowInactiveEffect;
                    break;
                case EnergyType.RED:
                    
                    currentGlowEffect = redGlowInactiveEffect;
                    break;
                case EnergyType.YELLOW:
                   
                    currentGlowEffect = yellowGlowInactiveEffect;
                    break;
            }
          
        }
        _renderer.material = currentGlowEffect;
    }
    
    public void UpdateHealthBar(float currentValue)
    { 
        currentGlowEffect.SetFloat("_HP", currentValue);
     //   hpBar.fillAmount = currentValue;
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
               _renderer.material = currentGlowEffect;
            }
        }
    }
    
    private void BlinkEffect()
    {
        blinkToggle = !blinkToggle;
        switch (energyType)
        {
            case EnergyType.GREEN:
                _renderer.material = blinkToggle ? greenGlowInactiveEffect : greenGlowEffect;
                break;
            case EnergyType.BLUE:
                _renderer.material = blinkToggle ? blueGlowInactiveEffect : blueGlowEffect;
                break;
            case EnergyType.RED:
                _renderer.material = blinkToggle ? redGlowInactiveEffect : redGlowEffect;
                break;
            case EnergyType.YELLOW:
                _renderer.material = blinkToggle ? yellowGlowInactiveEffect : yellowGlowEffect;
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
