// InputManager.cs

using System;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    public event Action OnLeftClick;
    public event Action<Vector2> OnMouseRightDrag;


    public event Action<Vector2> OnMouseMove;
    public event Action<float> OnMouseScroll;
    
    private Vector3 lastMousePosition;
    
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
        
        if (Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;
            OnMouseRightDrag?.Invoke(delta);
        }

        // --- On Mouse Move ---
        OnMouseMove?.Invoke(Input.mousePosition);

        // --- On Mouse wheel ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f) OnMouseScroll?.Invoke(scroll);

     
    }

}