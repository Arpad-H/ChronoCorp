
using System;
using UnityEngine;
using UnityEngine.Serialization;
using Util;

public enum CameraMode
{
    IsoGlide,
    StackedTower,
    CoverFlow,
    SpiralGrid
}

public class CameraController : MonoBehaviour
{
    
    [FormerlySerializedAs("camShowcase")] public TemporalLayerStack temporalLayerStack;
    public Camera cam;

    [Header("Stacked Tower Attributes")]
    private float currentAngle;
    private float currentHeight;


    private CameraMode cameraMode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
    }

    void Start()
    {
        if (cam == null) cam = FindObjectOfType<Camera>();
        InputManager.Instance.OnMouseRightDrag += HandleRightMouseDrag;
        InputManager.Instance.OnMouseScroll += HandleScrollEvent;
        temporalLayerStack.OnCameraModeChanged += CameraModeChanged;
        cameraMode = temporalLayerStack.cameraMode;
       
    }

    void HandleRightMouseDrag(Vector2 delta)
    {
        // movement along x,z plane
        Vector3 right = cam.transform.right;
        right.y = 0;
        right.Normalize();
        Vector3 forward = cam.transform.forward;
        forward.y = 0;
        forward.Normalize();
        Vector3 move = (right * delta.x + forward * delta.y) * BalanceProvider.Balance.cameraPanSpeed;
        transform.position -= move; // subtract to counter drag direction
    }

    private void HandleScrollEvent(float scroll)
    {
        switch (cameraMode)
        {
            case CameraMode.IsoGlide:
                transform.position += new Vector3(0, 0, 1) * (scroll * BalanceProvider.Balance.cameraZoomSpeed);
                break;

            case CameraMode.StackedTower:
                float angleSpeed = temporalLayerStack.angleStep;
                float heightStep = temporalLayerStack.heightStep;
                float radius = temporalLayerStack.radius;
                float heightOffset = temporalLayerStack.heightOffset;
                float lookAheadAngle = temporalLayerStack.lookAheadAngle;

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
                float lookAheadRad = (currentAngle + lookAheadAngle) * Mathf.Deg2Rad; 
                Vector3 lookTarget = new Vector3(
                    Mathf.Cos(lookAheadRad) * radius,
                    currentHeight,
                    Mathf.Sin(lookAheadRad) * radius
                );

                transform.LookAt(lookTarget);
                break;

            case CameraMode.CoverFlow:
                temporalLayerStack.UpdateCoverFlowFrames(scroll);
                break;
            case CameraMode.SpiralGrid:
                float newSize = cam.orthographicSize - scroll * BalanceProvider.Balance.cameraZoomSpeed;
                cam.orthographicSize = Math.Clamp(newSize ,BalanceProvider.Balance.spiralGridminMaxCameraY.x, BalanceProvider.Balance.spiralGridminMaxCameraY.y);
                break;
        }
    }

    public void ForceCamUpdate()
    {
        cameraMode = temporalLayerStack.cameraMode;
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
            case CameraMode.SpiralGrid:
                transform.position = new Vector3(0, 10, 0);
                break;
        }
    }

    public RaycastHit[] RaycastAll()
    {
        
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        return Physics.RaycastAll(ray, 1000f);
    }
    public bool RaycastForFirst(out RaycastHit hit)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
      //  Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red, 1f);
        hit = new RaycastHit();
        if( Physics.Raycast(ray, out hit,1000f)) return true;
        return false;
    }
}