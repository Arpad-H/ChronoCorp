// GameManager.cs
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton

    [Header("Game State")]
    public bool isGameOver = false;
    public float totalNetworkSupply = 0f;
    public float totalNetworkDemand = 0f;
    public float networkEfficiency = 1f;

    [Header("Layer Management")]
    public float layerDuplicationTime = 60f; 
    private float layerTimer = 0f;
    public float layerZSpacing = 15f; // How far apart to space layers
    public int currentLayerIndex = 0; // 0 is the present

    // Lists for the "Present" layer (Active GameObjects)
    public List<Node> presentNodes = new List<Node>();
    public List<Conduit> presentConduits = new List<Conduit>();

    // Data for "Past" layers (Data-only)
    public List<TimeLayerState> pastLayers = new List<TimeLayerState>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (isGameOver) return;

        // --- Layer Duplication Logic ---
        layerTimer += Time.deltaTime;
        if (layerTimer >= layerDuplicationTime)
        {
            DuplicateLayer();
            layerTimer = 0f;
        }

        // --- Simulation Logic ---
        SimulatePastLayers();
        SimulateTemporalConnections();
        SimulatePresentLayer();

        // --- UI Update ---
        UIManager.Instance.UpdateStatus(totalNetworkSupply, totalNetworkDemand, pastLayers.Count, layerTimer);
    }

    void SimulatePresentLayer()
    {
        // 1. Calculate total supply and demand for the PRESENT network
        totalNetworkSupply = 0f;
        totalNetworkDemand = 0f;

        foreach (Node node in presentNodes)
        {
            if (node.isSource)
                totalNetworkSupply += node.energySupply;
            else
                totalNetworkDemand += node.energyDemand;
        }

        // 2. Calculate network efficiency
        if (totalNetworkDemand <= 0)
            networkEfficiency = 1f;
        else
            networkEfficiency = Mathf.Clamp01(totalNetworkSupply / totalNetworkDemand);

        // 3. Tell each node to simulate itself based on this efficiency
        foreach (Node node in presentNodes)
        {
            node.SimulateStep(networkEfficiency);
        }
    }

    void SimulatePastLayers()
    {
        // Loop through each past layer and run its own simulation
        // This is a simplified simulation running on data, not GameObjects
        foreach (TimeLayerState layer in pastLayers)
        {
            layer.SimulateStep();
        }
    }

    void SimulateTemporalConnections()
    {
        // TODO: In Phase 2, this is where we would add/subtract energy
        // between layers before the present layer is simulated.
    }

    void DuplicateLayer()
    {
       
        TimeLayerState snapshot = CaptureCurrentState();
        pastLayers.Add(snapshot);

        // 2. Move the "Present" layer forward in Z space
        currentLayerIndex++;
        float newZ = currentLayerIndex * layerZSpacing;
        foreach (Node node in presentNodes)
        {
            node.transform.position = new Vector3(node.transform.position.x, node.transform.position.y, newZ);
        }
        // Conduits will follow their nodes automatically

        // 3. Tell the LayerVisualizer to draw the new past layer
        LayerVisualizer.Instance.RedrawPastLayers(pastLayers);

        Debug.Log($"New Layer created! Total layers: {pastLayers.Count + 1}");
    }

    TimeLayerState CaptureCurrentState()
    {
        TimeLayerState newState = new TimeLayerState(currentLayerIndex - 1);
        
        foreach (Node node in presentNodes)
        {
            // Create data copies
            NodeData data = new NodeData(node);
            newState.nodes.Add(data);
        }
        
        foreach (Conduit conduit in presentConduits)
        {
            ConduitData data = new ConduitData(conduit);
            newState.conduits.Add(data);
        }
        return newState;
    }

    // --- Public Game Functions ---

    public void AddNode(Node node)
    {
        presentNodes.Add(node);
    }

    public void AddConduit(Conduit conduit)
    {
        presentConduits.Add(conduit);
    }

    public void GameOver(string reason)
    {
        isGameOver = true;
        Time.timeScale = 0f; // Pause game
        Debug.LogError($"GAME OVER: {reason}");
        UIManager.Instance.ShowGameOver(reason);
    }
}