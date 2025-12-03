// InputManager.cs

using System;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    public event Action OnLeftClick;
    public event Action OnLeftClickUp;
    public event Action OnButtonN;
    public event Action OnButtonG;
    public event Action OnButtonX;
    public event Action OnButton1;
    public event Action OnButton2;
    public event Action OnButton3;
    public event Action OnButton4;
    public event Action OnButton5;
    public event Action OnButton6;
    public event Action OnButtonD;


    public event Action<Vector2> OnMouseMove;
    public event Action<float> OnMouseScroll;

    [SerializeField] private LayerMask nodeLayerMask = ~0;
    
    [SerializeField] private float tempLineWidth = 0.05f;

    public CameraController cameraController;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
    }

    void Start()
    {
        if (cameraController == null) cameraController = FindObjectOfType<CameraController>();
    }

    void Update()
    {
        // --- On Left Click ---
        if (Input.GetMouseButtonDown(0)) OnLeftClick?.Invoke();

        // --- On Mouse Move ---
        OnMouseMove?.Invoke(Input.mousePosition);

        // --- On Mouse wheel ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f) OnMouseScroll?.Invoke(scroll);

        // --- Button N ---
        if (Input.GetKeyDown(KeyCode.N)) OnButtonN?.Invoke();

        // --- Button G ---
        if (Input.GetKeyDown(KeyCode.G)) OnButtonG?.Invoke();

        // --- Button X ---
        if (Input.GetKeyDown(KeyCode.X)) OnButtonX?.Invoke();

        // --- Button D ---
        if (Input.GetKeyDown(KeyCode.D)) OnButtonD?.Invoke();

        // --- Buttons 1-6 ---
        if (Input.GetKeyDown(KeyCode.Alpha1)) OnButton1?.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha2)) OnButton2?.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha3)) OnButton3?.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha4)) OnButton4?.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha5)) OnButton5?.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha6)) OnButton6?.Invoke();
        
        // Check for Mouse Button Up 
        if (Input.GetMouseButtonUp(0)) OnLeftClickUp?.Invoke();
        
    }

}