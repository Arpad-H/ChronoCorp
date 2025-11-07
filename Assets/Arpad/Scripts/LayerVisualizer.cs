// LayerVisualizer.cs
using UnityEngine;
using System.Collections.Generic;

public class LayerVisualizer : MonoBehaviour
{
    public static LayerVisualizer Instance;

    [Header("Visual Prefabs")]
    public GameObject visualNodePrefab; // A simple Sprite-based prefab
    public GameObject visualConduitPrefab; // A prefab with just a LineRenderer

    private List<GameObject> visualObjects = new List<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    public void RedrawPastLayers(List<TimeLayerState> layers)
    {
        // Clear all old visuals
        foreach (GameObject obj in visualObjects)
        {
            Destroy(obj);
        }
        visualObjects.Clear();

      
        foreach (TimeLayerState layer in layers)
        {
            // Use a dictionary to find node positions by ID
            Dictionary<int, Vector3> nodePositions = new Dictionary<int, Vector3>();

            // Draw nodes
            foreach (NodeData node in layer.nodes)
            {
                GameObject nodeObj = Instantiate(visualNodePrefab, node.position, Quaternion.identity, transform);
                
                // Set color based on state
                SpriteRenderer sr = nodeObj.GetComponent<SpriteRenderer>();
                if (node.isSource)
                {
                    sr.color = Color.green;
                }
                else
                {
                    // Show if this past ripple is "failing"
                    sr.color = (node.currentEnergy < node.energyDemand * 0.99f) ? Color.red : Color.cyan;
                }
            
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.5f);

                visualObjects.Add(nodeObj);
                nodePositions.Add(node.id, node.position);
            }

            // Draw conduits
            foreach (ConduitData conduit in layer.conduits)
            {
                if (nodePositions.ContainsKey(conduit.nodeA_id) && nodePositions.ContainsKey(conduit.nodeB_id))
                {
                    GameObject conduitObj = Instantiate(visualConduitPrefab, transform);
                    LineRenderer lr = conduitObj.GetComponent<LineRenderer>();
                    
                    lr.SetPosition(0, nodePositions[conduit.nodeA_id]);
                    lr.SetPosition(1, nodePositions[conduit.nodeB_id]);
                    lr.startColor = lr.endColor = new Color(1, 1, 1, 0.3f); // Faint white

                    visualObjects.Add(conduitObj);
                }
            }
        }
    }
}