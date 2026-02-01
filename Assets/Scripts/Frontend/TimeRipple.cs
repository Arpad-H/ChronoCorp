using System.Collections;
using System.Collections.Generic;
using Backend.Simulation.World;
using NodeBase;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Util;

public class TimeRipple : NodeVisual
{
    public EnergyType energyType;

    [Header("Effects")]
    //  public Material greenGlowEffect;
    //  public Material blueGlowEffect;
    //  public Material redGlowEffect;
    //  public Material yellowGlowEffect;
    //  public Material greenGlowInactiveEffect;
    //  public Material blueGlowInactiveEffect;
    //  public Material redGlowInactiveEffect;
    //  public Material yellowGlowInactiveEffect;
    // private Material currentGlowEffect;
    // private Material lastGlowEffect;
    private Color colorNonGlow;
    private Color colorBlink;
    private Color colorBG;
    private Color colorGlow;
    public float glowIntensity = 2f;
    public float darkenIntensity = 0.25f;
    public GlowHP glow;
    private bool isBlinking = false;
    private bool blinkToggle;
    public GameObject ScreenEdgeIconPrefab;
    private ScreenEdgeIcon screenEdgeIcon;

    [Header("Other")] 
    public Renderer _renderer;
    public float currentHp = 1f;
    public NodeInfoWindow nodeInfoWindow;
    public UI_FollowObjecte nodeInfoWindowFollower;
        private bool isEnergySupplied = true;
        
    [Header("Scoring")]
    private Coroutine scoreRoutine;
    private float timeSinceLastValidHpThreshold = 0;
    
    [Header("Stat Panel")]
    private float energyConsumptionPerSecond = 0f;
    private float energyReceivedPerSecond = 0f;
    readonly Queue<(float time, float amount)> samples = new();
    public float EnergyPerSecond => energyReceivedPerSecond;
    float lastyEnergyPacket = -Mathf.Infinity;
    float windowStartTime;
    float energyInWindow;
    [SerializeField] float rateWindow = 2.0f;


    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        energyConsumptionPerSecond = BalanceProvider.Balance.nodeHealthDrain/(BalanceProvider.Balance.nodeDrainHealthEveryNTicks/60f);
        ChangeEnergySupplyState(false);
        UpdateHealthBar(1f);
    }
    protected override void ShowInfoWindow(bool show)
    {
        if (NodeInfoWindow.Instance == null) return;

        if (show)
        {
            NodeInfoWindow.Instance.Show(
                this, 
                backendID, 
                Mathf.RoundToInt(currentHp * 100), 
                energyConsumptionPerSecond, 
                energyReceivedPerSecond
            );
        }
        else
        {
            NodeInfoWindow.Instance.Hide();
        }
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
        colorNonGlow = energyType.ToColor();
        float factor = Mathf.Pow(2, glowIntensity);
        colorGlow = new Color(colorNonGlow.r * factor, colorNonGlow.g * factor, colorNonGlow.b * factor, 1f);
        colorBlink = Color.Lerp(colorNonGlow, Color.white, 0.5f);
        colorBG = colorNonGlow * darkenIntensity;
        glow.SetVisuals(currentHp, colorGlow);
        glow.SetBGColor(colorBG);
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
            colorNonGlow = energyType.ToColor();
            float factor = Mathf.Pow(2, glowIntensity);
            //colorGlow = new Color(colorNonGlow.r * factor, colorNonGlow.g * factor, colorNonGlow.b * factor, 1f);
            glow.SetVisuals(currentHp, colorNonGlow);
        }
    }

    public void UpdateHealthBar(float currentValue)
    {
      
        CalcEnergyMean(currentValue);
        currentHp = currentValue;
        EvaluateScore(currentValue);
        glow.SetHP(currentHp);
        // hpBar.fillAmount = currentValue;
        if (currentValue <= BalanceProvider.Balance.nodeBlinkThreshhold && !isEnergySupplied)
        {
            if (!screenEdgeIcon)
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

    private void CalcEnergyMean(float currentValue)
    {
        float delta = currentValue - currentHp;
        currentHp = currentValue;

        if (delta <= 0f) { RecalcEnergyMean(); return; }

        float now = Time.time;
        samples.Enqueue((now, BalanceProvider.Balance.energyPacketRechargeAmount));

        // drop old samples
        while (samples.Count > 0 && now - samples.Peek().time > rateWindow)
            samples.Dequeue();

        RecalcEnergyMean();
    }
    void RecalcEnergyMean()
    {
        float sum = 0f;
        foreach (var s in samples) sum += s.amount;
        energyReceivedPerSecond = sum / rateWindow;
    }

    private void EvaluateScore(float currentValue) 
    {
        bool isAboveThreshold = currentValue > BalanceProvider.Balance.hpThresholdForScoreBonus;
        if (isAboveThreshold && scoreRoutine == null && isEnergySupplied) {
            scoreRoutine = StartCoroutine(ScoreBonusRoutine());
        } 
        else if (!isAboveThreshold && scoreRoutine != null) {
            StopCoroutine(scoreRoutine);
            scoreRoutine = null;
        }
    }
    private IEnumerator ScoreBonusRoutine() 
    {
        while (true) {
            yield return new WaitForSeconds(BalanceProvider.Balance.scoreInterval);
            GameFrontendManager.Instance.AddScore(BalanceProvider.Balance.scorePerInterval);
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
            }
        }
    }

    private void BlinkEffect()
    {
        blinkToggle = !blinkToggle;
        if (blinkToggle) glow.SetBGColor(colorNonGlow);
        else glow.SetBGColor(colorBlink);
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

    public override void AddConnectedConduit(ConduitVisual conduitVisual, Direction dir)
    {
        isDirectionOccupied[dir] = conduitVisual;
        connectedConduits.Add(conduitVisual);
        ChangeEnergySupplyState(true);
    }
    public float getEnergyReceivedPerSecond()
    {
        return energyReceivedPerSecond;
    }
    public float getEnergyConsumptionPerSecond()
    {
        energyConsumptionPerSecond = BalanceProvider.Balance.nodeHealthDrain/(BalanceProvider.Balance.nodeDrainHealthEveryNTicks * 1f/SimulationStorage.TICKS_PER_SECOND);
        return energyConsumptionPerSecond;
    }
}