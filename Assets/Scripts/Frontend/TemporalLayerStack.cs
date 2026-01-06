using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class TemporalLayerStack : MonoBehaviour
{
    public Action<CameraMode> OnCameraModeChanged;
    public GameObject framePrefab;

    [Min(1)] public int numberOfFrames = 1;

    public CameraController cameraController;
    [SerializeField] public CameraMode cameraMode = CameraMode.IsoGlide;
    private CameraMode previousMode = CameraMode.StackedTower;

    [Header("Stacked Tower Settings")] public float heightStep = 4.0f; // vertical distance between frames
    public float radius = 5.0f; // distance from center pillar
    public float angleStep = 25f; // how many degrees each step advances
    public float heightOffset = 5f;
    public float lookAheadAngle = 15f;

    [Header("Cover Flow Settings")] public float spacing = 2f; // horizontal spacing
    public float sideAngle = 60f; // Y rotation for side planes
    public float sideZOffset = 0.5f; // depth offset for side planes
    public float sideScale = 0.8f; // scale for side planes
    public float scrollSpeed = 5f; // how fast scroll moves
    public float lerpSpeed = 10f; // how fast planes animate to positions
    private float centerIndex = 0f; // current "floating" center
    private int half;
    private Vector3 baseScale = new Vector3(16, 9, 1);
    [Header("Cam Settings")] private List<CoordinatePlane> frames = new List<CoordinatePlane>();

GameObject vfxPrefab;
    void Start()
    {
        GenerateFrames();
        OnCameraModeChanged?.Invoke(cameraMode);
        half = numberOfFrames / 2;
        ;
        baseScale = framePrefab.transform.localScale;
    }

    public void OnValidate()
    {
// #if UNITY_EDITOR
//         if (!Application.isPlaying)
//         {
//             EditorApplication.delayCall += () =>
//             {
//                 if (this != null)
//                 {
//                     GenerateFrames();
//                     cameraController.ForceCamUpdate();
//                 }
//             };
//         }
// #endif
    }

    void HandleCameraModeChanged(CameraMode newMode)
    {
        GenerateFrames();
    }

    private void Update()
    {
        if (cameraMode != previousMode)
        {
            previousMode = cameraMode;
            OnCameraModeChanged?.Invoke(cameraMode);
            HandleCameraModeChanged(cameraMode);
        }
    }

    private void GenerateFrames()
    {
        // Cleanup
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        frames.Clear();


        switch (cameraMode)
        {
            case CameraMode.IsoGlide:
                for (int i = 0; i < numberOfFrames; i++)
                {
                    Vector3 pos = new(0, 0, i * 10.0f);
                    CoordinatePlane frame = InstantiatePrefab(pos, Quaternion.identity, $"IsoGlideFrame_{i}");
                }

                break;

            case CameraMode.StackedTower:
                Vector3 center = Vector3.zero; // Center of the helix
                for (int i = 0; i < numberOfFrames; i++)
                {
                    float angle = i * angleStep * Mathf.Deg2Rad;

                    // Position in helix
                    Vector3 pos = center + new Vector3(
                        Mathf.Cos(angle) * radius,
                        i * heightStep,
                        Mathf.Sin(angle) * radius
                    );

                    Quaternion rot = Quaternion.Euler(90f, 0f, i * angleStep);

                    CoordinatePlane frame = InstantiatePrefab(pos, rot, $"SpiralFrame_{i}");
                    frame.layerNum = i;
                    frames.Add(frame);
                }

                break;

            case CameraMode.CoverFlow:
                // max Y rotation for side planes
                int half = numberOfFrames / 2;

                for (int i = 0; i < numberOfFrames; i++)
                {
                    int offsetFromCenter = i - half;

                    // X position
                    float xPos = offsetFromCenter * spacing;

                    // Z position: planes slightly behind as they go to the side
                    float zPos = Mathf.Abs(offsetFromCenter) * 0.5f;

                    // Rotation: tilt side planes along Y
                    float yRot = sideAngle * Mathf.Sign(offsetFromCenter) * Mathf.Min(Mathf.Abs(offsetFromCenter), 1f);

                    Vector3 pos = new Vector3(xPos, 0, zPos);
                    Quaternion rot = Quaternion.Euler(0, yRot, 0);

                    CoordinatePlane frame = InstantiatePrefab(pos, rot, $"CoverFlow_{i}");
                }

                break;
        }
    }
    IEnumerator DelaySpawn()
    {
       
        yield return new WaitForSeconds(5);

      
    }
    public CoordinatePlane AddNewFrame()
    {
        Instantiate(vfxPrefab);
       // StartCoroutine(DelaySpawn());
        Vector3 pos;
        CoordinatePlane frame = null;
        Quaternion rot;
        switch (cameraMode)
        {
            case CameraMode.IsoGlide:

                pos = new(0, 0, numberOfFrames * 10.0f);
                frame = InstantiatePrefab(pos, Quaternion.identity, $"IsoGlideFrame_{numberOfFrames}");
                break;

            case CameraMode.StackedTower:
                Vector3 center = Vector3.zero; // Center of the helix

                float angle = numberOfFrames * angleStep * Mathf.Deg2Rad;

                // Position in helix
                pos = center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    numberOfFrames * heightStep,
                    Mathf.Sin(angle) * radius
                );

                rot = Quaternion.Euler(90f, 0f, numberOfFrames * angleStep);

                frame = InstantiatePrefab(pos, rot, $"SpiralFrame_{numberOfFrames}");
                frame.layerNum = numberOfFrames;
                frames.Add(frame);


                break;

            case CameraMode.CoverFlow:
                // max Y rotation for side planes
                int half = numberOfFrames / 2;


                int offsetFromCenter = numberOfFrames - half;

                // X position
                float xPos = offsetFromCenter * spacing;

                // Z position: planes slightly behind as they go to the side
                float zPos = Mathf.Abs(offsetFromCenter) * 0.5f;

                // Rotation: tilt side planes along Y
                float yRot = sideAngle * Mathf.Sign(offsetFromCenter) * Mathf.Min(Mathf.Abs(offsetFromCenter), 1f);

                pos = new Vector3(xPos, 0, zPos);
                rot = Quaternion.Euler(0, yRot, 0);

                frame = InstantiatePrefab(pos, rot, $"CoverFlow_{numberOfFrames}");


                break;
        }

        if (frame) numberOfFrames += 1;
        return frame;
    }

    private CoordinatePlane InstantiatePrefab(Vector3 pos, Quaternion rot, string objName)
    {
        CoordinatePlane obj;

        if (Application.isPlaying)
            obj = Instantiate(framePrefab, pos, rot, transform).GetComponent<CoordinatePlane>();
        else
            obj = ((GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(framePrefab, transform))
                .GetComponent<CoordinatePlane>();

        obj.transform.SetLocalPositionAndRotation(pos, rot);
        obj.name = objName;

        frames.Add(obj);
        return obj;
    }

    public void UpdateFrames(float scrollDelta)
    {
        if (cameraMode != CameraMode.CoverFlow) return;

        // Move center index
        centerIndex -= scrollDelta * scrollSpeed;
        centerIndex = Mathf.Clamp(centerIndex, 0, numberOfFrames - 1);

        for (int i = 0; i < numberOfFrames; i++)
        {
            float offset = i - centerIndex;

            // Target position
            float targetX = offset * spacing;
            float targetZ = Mathf.Abs(offset) * sideZOffset;
            Vector3 targetPos = new Vector3(targetX, 0, targetZ);

            // Target rotation
            float targetYRot = (Mathf.Abs(offset) < 0.01f)
                ? 0f
                : sideAngle * Mathf.Sign(offset) * Mathf.Min(Mathf.Abs(offset), 1f);
            Quaternion targetRot = Quaternion.Euler(0f, targetYRot, 0f);

            // Target scale
            float targetScale = (Mathf.Abs(offset) < 0.01f)
                ? 1f
                : Mathf.Lerp(1f, sideScale, Mathf.Min(Mathf.Abs(offset), 1f));
            Vector3 targetScl = baseScale * targetScale;

            // DIRECTLY APPLY (no smoothing)
            frames[i].transform.localPosition = targetPos;
            frames[i].transform.localRotation = targetRot;
            frames[i].transform.localScale = targetScl;
        }
    }
}