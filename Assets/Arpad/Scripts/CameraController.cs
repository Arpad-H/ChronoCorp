using System;
using System.Collections.Generic;
using UnityEngine;

public enum CameraMode
{
    IsoGlide,
    StackedTower,
    CoverFlow
}

public class CameraController : MonoBehaviour
{
    public CamShowcase camShowcase;
    public float zoomSpeed = 3f;
    public Camera cam;

    [Header("Stacked Tower Attributes")] private float currentAngle;
    private float currentHeight;


    private CameraMode cameraMode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
    }

    void Start()
    {
        if (cam == null) cam = FindObjectOfType<Camera>();
        InputManager.Instance.OnLeftClick += HandleClickEvent;
        InputManager.Instance.OnMouseScroll += HandleScrollEvent;
        camShowcase.OnCameraModeChanged += CameraModeChanged;
        cameraMode = camShowcase.cameraMode;
    }

    void OnDisable()
    {
        InputManager.Instance.OnLeftClick -= HandleClickEvent;
    }

    void HandleClickEvent()
    {
        // Handle click event based on camera mode
    }

    private void HandleScrollEvent(float scroll)
    {
        switch (cameraMode)
        {
            case CameraMode.IsoGlide:
                transform.position += new Vector3(0, 0, 1) * (scroll * zoomSpeed);
                break;

            case CameraMode.StackedTower:
                float angleSpeed = camShowcase.angleStep;
                float heightStep = camShowcase.heightStep;
                float radius = camShowcase.radius;
                float heightOffset = camShowcase.heightOffset;
                float lookAheadAngle = camShowcase.lookAheadAngle;

                // Advance angle and height based on scroll
                currentAngle += scroll * angleSpeed;
                currentHeight += scroll * heightStep;

                float rad = currentAngle * Mathf.Deg2Rad;

                // Spiral position around the pillar
                Vector3 spiralPos = new Vector3(
                    Mathf.Cos(rad) * radius,
                    currentHeight,
                    Mathf.Sin(rad) * radius
                );

                transform.position = spiralPos + new Vector3(0, heightOffset, 0);

                // Rotate camera so it both looks toward center and orbits Y
                // Option 1: always face slightly ahead along the spiral
                float lookAheadRad = (currentAngle + lookAheadAngle) * Mathf.Deg2Rad; // tweak 15° for smooth look
                Vector3 lookTarget = new Vector3(
                    Mathf.Cos(lookAheadRad) * radius,
                    currentHeight,
                    Mathf.Sin(lookAheadRad) * radius
                );

                transform.LookAt(lookTarget);
                break;

            case CameraMode.CoverFlow:
                camShowcase.UpdateFrames(scroll);
                break;
        }
    }

    public void ForceCamUpdate()
    {
        cameraMode = camShowcase.cameraMode;
        HandleScrollEvent(0);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void CameraModeChanged(CameraMode mode)
    {
        cameraMode = mode;
        switch (cameraMode)
        {
            case CameraMode.IsoGlide:
                transform.position = new Vector3(-13, 16, -20);
                transform.rotation = Quaternion.Euler(30, 38, 0);
                break;

            case CameraMode.StackedTower:
                HandleScrollEvent(0);
                break;

            case CameraMode.CoverFlow:
                transform.position = new Vector3(0, 0, -20);
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
        }
    }

    public RaycastHit[] RaycastAll()
    {
        
        // RaycastHit[] hits;
        // Vector3 origin = transform.position;
        // Vector3 direction = transform.forward;
        // float distance =1000.0F;
        // //debug with drawing the ray
        // Debug.DrawRay(origin, direction * distance, Color.red, 10.0f);
        // hits = Physics.RaycastAll(transform.position, transform.forward, 1000.0F);
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red, 1f);
        return Physics.RaycastAll(ray, 1000f);
    }
}