using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class GeneratorInfoWindow : MonoBehaviour
{
    public static GeneratorInfoWindow Instance;
    [Header("UI References")]
    public TextMeshProUGUI nodeNameText;
    public TextMeshProUGUI nodeTierText;
    public TextMeshProUGUI nodeOutputText;
    public TextMeshProUGUI nodeConnectionsText;
    public TextMeshProUGUI cursorUnderscoreObject;
    public Canvas parentCanvas;
    public Camera mainCamera;
    
    [SerializeField] Color lowColor = Color.red;
    [SerializeField] Color goodColor = Color.green;
    [SerializeField] Color terminalColor = Color.black;
    public float blinkInterval = 0.5f;
    private float blinkTimer = 0f;
    private bool isCursorVisible = true;
    
    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 2f, 0);
    private RectTransform myRectTransform;
    private Generator targetNodeVisual;
   

    
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
      
    }
    public void Show(Generator nodeVisual, Guid id, int tier, float output, int connections)
    {
        targetNodeVisual = nodeVisual;
        
        SetTexts(id, tier, output, connections);
        gameObject.SetActive(true);
        UpdatePosition();
    }

    private void SetTexts(Guid id, int tier, float output, int connections)
    {
        nodeNameText.text =   ">SYSTEM_SCAN #" + id.ToString("N")[..5];
        nodeConnectionsText.text = connections + " / " + tier;
        SetTier(tier);
        nodeOutputText.text = output.ToString("F2");
    }

    private void SetTier(int tier)
    {
        char hpChar = '■';
        char emptyHpChar = '□';
        int totalChars = 4;
        string tierString = new string(hpChar, tier) + new string(emptyHpChar, totalChars - tier);
        nodeTierText.text = tierString;
    }

    public void Hide()
    {
     
        cursorUnderscoreObject.text = "> ";
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
        
        SetTexts( targetNodeVisual.backendID, targetNodeVisual.GetTier(), targetNodeVisual.GetOutput(),targetNodeVisual.GetConnectedConduitCount());
        
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
   
    
}