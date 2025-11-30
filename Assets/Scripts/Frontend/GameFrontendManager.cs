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
    public static GameFrontendManager Instance; // Singleton
    public CameraController cameraController;

    [Header("Asset References")] public GameObject nodePrefab;
    public GameObject generatorPrefab;
    public GameObject conduitPrefab;


    private IBackend backend; // Link to backend

    private long fixedTickCount = 0;

    //TODO Layer Management via backend
    [Header("Layer Management")] public float layerDuplicationTime = 60f;
    private float layerTimer = 0f;
    public float layerZSpacing = 15f; // How far apart to space layers
    //public List<TimeLayerState> temporalLayers = new List<TimeLayerState>(); // were gonan get this from backend


    [Header("Energy Management")]
    //TODO replaced with backend link later
    private EnergyNetworkManager energyNetworkManager = new EnergyNetworkManager();


    void Awake()
    {
        backend = new BackendImpl(this);
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
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
        backend.tick(fixedTickCount);
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

        // Avoid duplicate conduits between the same nodes
        foreach (var c in energyNetworkManager.presentConduits)
        {
            if ((c.nodeVisualA == a && c.nodeVisualB == b) || (c.nodeVisualA == b && c.nodeVisualB == a))
                return; // Already connected
        }

        GameObject conduitObj = Instantiate(conduitPrefab, Vector3.zero, Quaternion.identity);
        Conduit conduit = conduitObj.GetComponent<Conduit>();
        conduit.Initialize(a, b);
        energyNetworkManager.AddConduit(conduit);
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
        throw new NotImplementedException();
    }

    public bool AddTimeSlice(int sliceNum)
    {
        throw new NotImplementedException();
    }
}