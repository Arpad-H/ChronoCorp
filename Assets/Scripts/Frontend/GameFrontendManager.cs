// GameManager.cs

using System;
using UnityEngine;
using System.Collections.Generic;
using Backend.Simulation.World;
using Interfaces;
using NodeBase;
using UnityEditor;
using NodeType = NodeBase.NodeType;


public class GameFrontendManager : MonoBehaviour, Interfaces.IFrontend
{
    public static GameFrontendManager Instance; 
    public CameraController cameraController;
    private EnergyPacketVisualizer energyPacketVisualizer;
    private IBackend backend; // Link to backend
    
    [Header("Asset References")] public GameObject nodePrefab;
    public GameObject generatorPrefab;
    public GameObject conduitPrefab;

    public CoordinatePlane layer0 ; //TODO temp hardcode
    private Dictionary<int, CoordinatePlane> layerToCoordinatePlane = new Dictionary<int, CoordinatePlane>();

    private long fixedTickCount = 0;

    //TODO Layer Management via backend
    [Header("Layer Management")] public float layerDuplicationTime = 60f;
    private float layerTimer = 0f;

    public float layerZSpacing = 15f; // How far apart to space layers


    void Awake()
    {
        backend = new BackendImpl(this);
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        
        if (energyPacketVisualizer == null) energyPacketVisualizer = FindObjectOfType<EnergyPacketVisualizer>();
        if (cameraController == null) cameraController = FindObjectOfType<CameraController>();
        InputManager.Instance.OnButtonN += () => SpawnManuallyOnHoveredFrame(NodeDTO.RIPPLE, EnergyType.BLUE);
        InputManager.Instance.OnButtonG += () => SpawnManuallyOnHoveredFrame(NodeDTO.GENERATOR, EnergyType.BLUE);
        InputManager.Instance.OnButtonX += () => DeleteNodeManually();

        // InputManager.Instance.OnButton1 += () => SpawnOnHoveredFrame(nodePrefab);
        // InputManager.Instance.OnButton2 += () => SpawnOnHoveredFrame(nodePrefab);
        // InputManager.Instance.OnButton3 += () => SpawnOnHoveredFrame(nodePrefab);
        // InputManager.Instance.OnButton4 += () => SpawnOnHoveredFrame(nodePrefab);
        // InputManager.Instance.OnButton5 += () => SpawnOnHoveredFrame(nodePrefab);
        // InputManager.Instance.OnButton6 += () => SpawnOnHoveredFrame(nodePrefab);
    }

    void Update()
    {
    }

    void FixedUpdate()
    {
        backend.tick(fixedTickCount, this);
        fixedTickCount++;
    }

    //when a button is pressed to spawn a node
    private bool SpawnManuallyOnHoveredFrame(NodeDTO nodeType, EnergyType energyType)
    {
        RaycastHit rh;
        cameraController.RaycastForFirst(out rh); //maybe replace with single ray with custom layer?

        CoordinatePlane frame = rh.transform.GetComponentInParent<CoordinatePlane>();
        if (frame == null) return false; // Not hovering over a frame
        int frameNum = frame.layerNum;
        Vector3 hitPoint = rh.point;

        Vector3 spawnPos = frame.WorldToLocal(hitPoint);
        Vector2 localCoordinates = frame.SnapToGrid(spawnPos);

        GUID? nodeBackendID = backend.PlaceNode(nodeType, frameNum, localCoordinates);
        if (nodeBackendID != null)
        {
            GameObject node = nodeType == NodeDTO.RIPPLE ? nodePrefab : generatorPrefab;
            frame.PlaceNode(node, spawnPos, out GameObject newNode);
            newNode.GetComponent<NodeVisual>().backendID = nodeBackendID.Value;
            return true;
        }


        return false;
    }

    private bool SpawnOnHoveredFrame(GameObject gameObject, EnergyType energyType)
    {
        RaycastHit rh;
        cameraController.RaycastForFirst(out rh); //maybe replace with single ray with custom layer?

        CoordinatePlane frame = rh.transform.GetComponentInParent<CoordinatePlane>();
        if (frame == null) return false; // Not hovering over a frame

        Vector3 hitPoint = rh.point;
        if (frame != null)
        {
            Vector3 spawnPos = frame.WorldToLocal(hitPoint);
            if (frame.PlaceNode(gameObject, spawnPos, out GameObject newNode)) return true;
        }


        return false;
    }

    private void DeleteNodeManually()
    {
        RaycastHit rh;
        cameraController.RaycastForFirst(out rh); //maybe replace with single ray with custom layer?

        NodeVisual node = rh.transform.GetComponentInParent<NodeVisual>();
        if (node == null) return;
        if (backend.DeleteNode(node.backendID))
        {
            Destroy(node.gameObject);
        }
    }

    public void SpawnConduit(NodeVisual a, NodeVisual b)
    {
        if (a == null || b == null || a == b) return;
        // GUID? connectionID;
        if (backend.LinkNodes(a.backendID, b.backendID, out GUID? connectionID))
        {
            GameObject conduitObj = Instantiate(conduitPrefab, Vector3.zero, Quaternion.identity);
            ConduitVisual conduitVisual = conduitObj.GetComponent<ConduitVisual>();
            conduitVisual.Initialize(a, b);
        }
    }


    public void GameOver(string reason)
    {
        Time.timeScale = 0f; // Pause game
        Debug.LogError($"GAME OVER: {reason}");
        UIManager.Instance.ShowGameOver(reason);
    }

    public bool PlaceNodeVisual(AbstractNodeInstance node, int layerNum, Vector2 planePos)
    {
        switch (node.NodeType.getShape())
        {
            case (Shape.CIRCLE):
                if (SpawnOnHoveredFrame(nodePrefab, EnergyType.BLUE)) return true;
                return false; //TODO change energy type based on color

            case (Shape.SQUARE):
                if (SpawnOnHoveredFrame(generatorPrefab, EnergyType.BLUE)) return true;
                return false; //TODO change energy type based on color
        }

        return false;
    }


    public void SpawnEnergyPacket(GUID guid)
    {
        energyPacketVisualizer.SpawnEnergyPacket(guid, backend);
    }
    public void DeleteEnergyPacket(GUID guid)
    {
       energyPacketVisualizer.DeleteEnergyPacket(guid);
    }

    public bool AddTimeSlice(int sliceNum)
    {
        throw new NotImplementedException();
    }

    public CoordinatePlane GetCoordinatePlane( int startPosLayer)
    {
        if (layer0 == null)
        {
            layer0 = GameObject.Find("SpiralFrame_0").GetComponent<CoordinatePlane>();
        }
        return layer0;
    }
}