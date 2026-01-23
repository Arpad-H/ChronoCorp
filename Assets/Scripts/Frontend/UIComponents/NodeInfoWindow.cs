using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class NodeInfoWindow : MonoBehaviour
{
    public static NodeInfoWindow Instance;
    [Header("UI References")]
    public TextMeshProUGUI nodeNameText;
    public TextMeshProUGUI nodeHPtext;
    public TextMeshProUGUI nodeEnergyDrainText;
    public TextMeshProUGUI nodeEnergyRecievedText;
    public TextMeshProUGUI cursorUnderscoreObject;
    public Canvas parentCanvas;
    public Camera mainCamera;
    public RawImage terminalImage;
    public RenderTexture rickRollRenderTexture;
    public Texture defaultTerminalTexture;
    public VideoPlayer videoPlayer; 
    [SerializeField] Color lowColor = Color.red;
    [SerializeField] Color goodColor = Color.green;
    [SerializeField] Color terminalColor = Color.black;
    public float blinkInterval = 0.5f;
    private float blinkTimer = 0f;
    private bool isCursorVisible = true;


    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 2f, 0);

    private RectTransform myRectTransform;
    private TimeRipple targetNodeVisual;
    private bool isTyping = false;
    public FakeTerminalAutoTyper fakeTerminalAutoTyper;
    // private float nodeHp;
    // float nodeEnergyDrain;
    // float nodeEnergyReceived;
    // Guid nodeId;
    
    void Awake()
    {
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
        fakeTerminalAutoTyper.OnExecuteConfirmed += () =>
        {
            PlayRickroll(); 
        };
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
        SetHP(hp);
        nodeEnergyDrainText.text =   energyDrain.ToString("F2");
        nodeEnergyRecievedText.text = energyReceived.ToString("F2");

        nodeEnergyRecievedText.color =
            energyReceived < energyDrain ? lowColor : goodColor;
    }

    private void SetHP(float hp)
    {
        char hpChar = '■';
        char emptyHpChar = '□';
        int totalChars = 10;
        int filledChars = Mathf.Clamp(Mathf.RoundToInt((hp / 100f) * totalChars), 0, totalChars);
        string hpBar = new string(hpChar, filledChars) + new string(emptyHpChar, totalChars - filledChars);
        nodeHPtext.text = hpBar;
    }

    public void Hide()
    {
        terminalImage.color = terminalColor;
            terminalImage.texture = defaultTerminalTexture;
            videoPlayer.Stop();
        cursorUnderscoreObject.text = "> ";
            isTyping = false;
            fakeTerminalAutoTyper.ResetTyping();
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

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            isTyping = true;
            fakeTerminalAutoTyper.AdvanceTyping();
            if (fakeTerminalAutoTyper.IsFinished)
            {
                PlayRickroll();
                isTyping = false;
            }
            
        }
    }

    void LateUpdate()
    {
        
        SetTexts(targetNodeVisual.backendID, 
            Mathf.RoundToInt(targetNodeVisual.currentHp * 100), 
            targetNodeVisual.getEnergyConsumptionPerSecond(), 
            targetNodeVisual.getEnergyReceivedPerSecond());

        if (isTyping)
        {
            return;
        }
        blinkTimer += Time.deltaTime;
        if (blinkTimer >= blinkInterval)
        {
            isCursorVisible = !isCursorVisible;
            if (isCursorVisible)
            {
                cursorUnderscoreObject.text = ">_";
            }
            else
            {
                cursorUnderscoreObject.text = "> ";
            }
            blinkTimer = 0f;
        }
    }
    public void PlayRickroll()
    {
        terminalImage.texture = rickRollRenderTexture;
        terminalImage.color = Color.white;
        videoPlayer.Play();
    }
    
}