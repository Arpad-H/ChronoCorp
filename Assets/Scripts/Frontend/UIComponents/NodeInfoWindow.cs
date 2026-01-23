using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NodeInfoWindow : MonoBehaviour
{
    public static NodeInfoWindow Instance;
    [Header("UI References")]
    public TextMeshProUGUI nodeNameText;
    public TextMeshProUGUI nodeHPtext;
    public TextMeshProUGUI nodeEnergyDrainText;
    public TextMeshProUGUI nodeEnergyRecievedText;
   
    public float blinkInterval = 0.5f;
    private float blinkTimer = 0f;
    private bool isCursorVisible = true;
    public GameObject cursorUnderscoreObject;   
    
    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 2f, 0);
    public Canvas parentCanvas;
    public Camera mainCamera;
    private RectTransform myRectTransform;
    private TimeRipple targetNodeVisual;
    
    // private float nodeHp;
    // float nodeEnergyDrain;
    // float nodeEnergyReceived;
    // Guid nodeId;
    
    void Awake()
    {
        cursorUnderscoreObject.SetActive(true);
        
        if (Instance != null && Instance != this) 
        { 
            Destroy(this.gameObject); 
        } 
        else 
        { 
            Instance = this; 
        }
        // Cache components
        myRectTransform = GetComponent<RectTransform>();
        // Start hidden
        gameObject.SetActive(false); 
    }
    public void Show(TimeRipple nodeVisual, Guid id, float hp, float energyDrain, float energyReceived)
    {
        targetNodeVisual = nodeVisual;
     
        // nodeEnergyDrain = energyDrain;
        // nodeEnergyReceived = energyReceived;
        // nodeHp = hp;
        // nodeId = id;
        
        SetTexts(id, hp, energyDrain, energyReceived);
        gameObject.SetActive(true);
        UpdatePosition();
    }

    private void SetTexts(Guid id, float hp, float energyDrain, float energyReceived)
    {
        nodeNameText.text = ">SYSTEM_SCAN " + id.ToString("N")[..5];
        nodeHPtext.text =  hp.ToString(CultureInfo.CurrentCulture);
        nodeEnergyDrainText.text =   energyDrain.ToString("F2");
        nodeEnergyRecievedText.text =energyReceived.ToString("F2");
    }

    public void Hide()
    {
        targetNodeVisual = null;
        gameObject.SetActive(false);
    }

   
    void UpdatePosition()
    {
        if (targetNodeVisual == null) return;
        // World -> Screen (pixels) using the camera that renders the 3D world
        Vector3 screenPos = mainCamera.WorldToScreenPoint(targetNodeVisual.transform.position + offset);

        // Optional: hide when behind camera
        if (screenPos.z < 0f)
        {
            gameObject.SetActive(false);
            return;
        }
        else if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        // Screen -> Canvas local
        RectTransform canvasRect = parentCanvas.transform as RectTransform;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            (Vector2)screenPos,
            parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera,
            out localPoint
        );

        myRectTransform.anchoredPosition = localPoint;
    }
    void LateUpdate()
    {
        
        SetTexts(targetNodeVisual.backendID, 
            Mathf.RoundToInt(targetNodeVisual.currentHp * 100), 
            targetNodeVisual.energyConsumptionPerSecond, 
            targetNodeVisual.energyReceivedPerSecond);
        
        blinkTimer += Time.deltaTime;
        if (blinkTimer >= blinkInterval)
        {
            cursorUnderscoreObject.SetActive(!cursorUnderscoreObject.activeSelf);
            blinkTimer = 0f;
        }
    }
    
}