using System.Collections.Generic;
using UnityEngine;

public class ConduitManager : MonoBehaviour
{
    public static ConduitManager Instance; 
    public GameObject conduitPrefab;
    private GameObject currentConduit; // the conduit being created
    private Conduit currentConduitScript;
    
    private List<GameObject> allConduits = new List<GameObject>();
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void Start()
    {
        InputManager.Instance.OnMouseMove += UpdateMouseWorldPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void RegisterNodeEvents(Node node)
    {
     //   node.OnConduitDragStarted += AddConduit;
    //    node.OnConduitDragEnded += AddConduit;
    }
    public void AddConduit(Node node)
    {
        currentConduit = Instantiate(conduitPrefab, node.GetAttachPosition(), transform.rotation);
        currentConduitScript = currentConduit.GetComponent<Conduit>();
        currentConduitScript.SetStartNode(node);
    }
    public void FinishConduit(Node node)
    {
        currentConduitScript.FinalizeConduit(node);
        allConduits.Add(currentConduit);
        
        currentConduit = null;
        currentConduitScript = null;
    }
    private void UpdateMouseWorldPosition(Vector2 mousePos)
    {
        if (currentConduitScript != null)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y));
            Debug.Log(worldPos);
            UpdateDragPosition(worldPos);
        }
    }
    public void UpdateDragPosition(Vector3 position)
    {
        currentConduitScript.UpdateDragPosition(position);
    }
}