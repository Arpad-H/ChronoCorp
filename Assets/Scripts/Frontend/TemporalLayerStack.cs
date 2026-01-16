using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Util;


public class TemporalLayerStack : MonoBehaviour
{
    public Action<CameraMode> OnCameraModeChanged;
    public GameObject framePrefab;

    [Min(1)] public int numberOfFrames = 1;

    public CameraController cameraController;
    [SerializeField] public CameraMode cameraMode = CameraMode.IsoGlide;
    private CameraMode previousMode;

    [Header("Stacked Tower Settings")] public float heightStep = 4.0f; // vertical distance between frames
    public float radius = 5.0f; // distance from center pillar
    public float angleStep = 25f; // how many degrees each step advances
    public float heightOffset = 5f;
    public float lookAheadAngle = 15f;

    [Header("Cover Flow Settings")] public float coverFlowSpacing = 2f; // horizontal spacing
    public float sideAngle = 60f; // Y rotation for side planes
    public float sideZOffset = 0.5f; // depth offset for side planes
    public float sideScale = 0.8f; // scale for side planes
    public float scrollSpeed = 5f; // how fast scroll moves
    public float lerpSpeed = 10f; // how fast planes animate to positions
    private float centerIndex = 0f; // current "floating" center

    [Header("SpiralGrid Settings")] public float spiralSpacing = 10f; // spacing between frames in the grid
    public Vector2Int debugCellCount = new Vector2Int(16, 9);


    private Vector3 baseScale = new Vector3(16, 9, 1);

    private Dictionary<int, CoordinatePlane> layerToCoordinatePlane = new();
   

    private void Awake()
    {
        previousMode = cameraMode;
    }

    void Start()
    {
        //GenerateFrames();
        OnCameraModeChanged?.Invoke(cameraMode);
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
       // GenerateFrames();
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

        layerToCoordinatePlane.Clear();


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
                    layerToCoordinatePlane.Add(i, frame);
                }

                break;

            case CameraMode.CoverFlow:
                // max Y rotation for side planes
                int half = numberOfFrames / 2;

                for (int i = 0; i < numberOfFrames; i++)
                {
                    int offsetFromCenter = i - half;

                    // X position
                    float xPos = offsetFromCenter * coverFlowSpacing;

                    // Z position: planes slightly behind as they go to the side
                    float zPos = Mathf.Abs(offsetFromCenter) * 0.5f;

                    // Rotation: tilt side planes along Y
                    float yRot = sideAngle * Mathf.Sign(offsetFromCenter) * Mathf.Min(Mathf.Abs(offsetFromCenter), 1f);

                    Vector3 pos = new Vector3(xPos, 0, zPos);
                    Quaternion rot = Quaternion.Euler(0, yRot, 0);

                    CoordinatePlane frame = InstantiatePrefab(pos, rot, $"CoverFlow_{i}");
                }

                break;
            case CameraMode.SpiralGrid:
                for (int i = 0; i < numberOfFrames; i++)
                {
                    Vector2Int spiralPos = GetSpiralCoordinates(i);
                    //  Vector2Int cellCount = BalanceProvider.Balance.layerGridCellCount;
                    Vector3 pos = new Vector3(spiralPos.x * debugCellCount.x + spiralPos.x * spiralSpacing, 0,
                        spiralPos.y * debugCellCount.y + spiralPos.y * spiralSpacing);
                    Quaternion rot = Quaternion.Euler(90, 0, 0);
                    CoordinatePlane frame =
                        InstantiatePrefab(pos, rot, $"SpiralGridFrame_{i}");
                }

                break;
        }
    }

    public CoordinatePlane AddNewFrame(int sliceNum)
    {
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
                break;

            case CameraMode.CoverFlow:
                // max Y rotation for side planes
                int half = numberOfFrames / 2;


                int offsetFromCenter = numberOfFrames - half;

                // X position
                float xPos = offsetFromCenter * coverFlowSpacing;

                // Z position: planes slightly behind as they go to the side
                float zPos = Mathf.Abs(offsetFromCenter) * 0.5f;

                // Rotation: tilt side planes along Y
                float yRot = sideAngle * Mathf.Sign(offsetFromCenter) * Mathf.Min(Mathf.Abs(offsetFromCenter), 1f);

                pos = new Vector3(xPos, 0, zPos);
                rot = Quaternion.Euler(0, yRot, 0);

                frame = InstantiatePrefab(pos, rot, $"CoverFlow_{numberOfFrames}");


                break;

            case CameraMode.SpiralGrid:

                Vector2Int spiralPos = GetSpiralCoordinates(numberOfFrames);
                //  Vector2Int cellCount = BalanceProvider.Balance.layerGridCellCount;
                pos = new Vector3(spiralPos.x * debugCellCount.x + spiralPos.x * spiralSpacing, 0,
                    spiralPos.y * debugCellCount.y + spiralPos.y * spiralSpacing);
                rot = Quaternion.Euler(90, 0, 0);
                frame = InstantiatePrefab(pos, rot, $"SpiralGridFrame_{numberOfFrames}");
                break;
        }

        if (frame)
        {
            frame.layerNum = sliceNum;
            layerToCoordinatePlane.Add(sliceNum, frame); 
            numberOfFrames += 1;
        }
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
        return obj;
    }

    public void UpdateCoverFlowFrames(float scrollDelta)
    {
        // if (cameraMode != CameraMode.CoverFlow) return;
        //
        // // Move center index
        // centerIndex -= scrollDelta * scrollSpeed;
        // centerIndex = Mathf.Clamp(centerIndex, 0, numberOfFrames - 1);
        //
        // for (int i = 0; i < numberOfFrames; i++)
        // {
        //     float offset = i - centerIndex;
        //
        //     // Target position
        //     float targetX = offset * coverFlowSpacing;
        //     float targetZ = Mathf.Abs(offset) * sideZOffset;
        //     Vector3 targetPos = new Vector3(targetX, 0, targetZ);
        //
        //     // Target rotation
        //     float targetYRot = (Mathf.Abs(offset) < 0.01f)
        //         ? 0f
        //         : sideAngle * Mathf.Sign(offset) * Mathf.Min(Mathf.Abs(offset), 1f);
        //     Quaternion targetRot = Quaternion.Euler(0f, targetYRot, 0f);
        //
        //     // Target scale
        //     float targetScale = (Mathf.Abs(offset) < 0.01f)
        //         ? 1f
        //         : Mathf.Lerp(1f, sideScale, Mathf.Min(Mathf.Abs(offset), 1f));
        //     Vector3 targetScl = baseScale * targetScale;
        //
        //     // DIRECTLY APPLY (no smoothing)
        //     frames[i].transform.localPosition = targetPos;
        //     frames[i].transform.localRotation = targetRot;
        //     frames[i].transform.localScale = targetScl;
        // }
    }

    public CoordinatePlane GetLayerByNum(int nodeVisualLayerNum)
    {
        if (layerToCoordinatePlane.ContainsKey(nodeVisualLayerNum))
            return layerToCoordinatePlane[nodeVisualLayerNum];

        return null;
    }

    private Vector2Int GetSpiralCoordinates(int n)
    {
        if (n == 0)
            return Vector2Int.zero;
        n += 1;//0 based makes it spawn in the corner first. like this it spanws on the right side first
        int layer = Mathf.FloorToInt((Mathf.Sqrt(n) + 1f) / 2f);
        int legLen = layer * 2;
        int legPos = n - (2 * layer - 1) * (2 * layer - 1);

        int x = 0;
        int y = 0;

        if (legPos < legLen)
        {
            // Right edge (going down)
            x = layer;
            y = layer - legPos;
        }
        else if (legPos < legLen * 2)
        {
            // Bottom edge (going left)
            x = layer - (legPos - legLen);
            y = -layer;
        }
        else if (legPos < legLen * 3)
        {
            // Left edge (going up)
            x = -layer;
            y = -layer + (legPos - legLen * 2);
        }
        else
        {
            // Top edge (going right)
            x = -layer + (legPos - legLen * 3);
            y = layer;
        }

        return new Vector2Int(x, y);
    }
}