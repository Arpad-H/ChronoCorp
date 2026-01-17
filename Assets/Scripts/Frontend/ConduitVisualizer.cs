using System.Collections.Generic;
using NodeBase;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

public class ConduitVisualizer : MonoBehaviour
{
    public static ConduitVisualizer Instance;
    public CameraController cameraController;
    public GameObject prefab;
    private ObjectPool<ConduitVisual> pool;

    private Dictionary<GUID, ConduitVisual> conduitVisuals = new();

    //private LinkedList<ConduitVisual> conduitVisuals = new();
    ConduitVisual previewConduitVisual;

    private void Awake()
    {
        Instance = this;
        pool = new ObjectPool<ConduitVisual>(
            CreateItem,
            OnGet,
            OnRelease,
            OnDestroyItem,
            true, // helps catch double-release mistakes
            10,
            100
        );
    }

    private void Start()
    {
        //  InputManager.Instance.OnLeftClickUp += CancelDrag;
        if (cameraController == null) cameraController = FindObjectOfType<CameraController>();
        GameFrontendManager.Instance.BackendDeletesConnection += OnDeleteConduit;
        GameFrontendManager.Instance.BackendCreatesConnection += OnBackendCreatesConnection;
    }

    private void OnBackendCreatesConnection(GUID backendIdA, GUID backendIdB, GUID connectionId,
        Vector2Int[] cellsOfConnection)
    {
        ConduitVisual conduitVisual = pool.Get();
        conduitVisual.InitializeNewConduit(backendIdA, backendIdB, connectionId, cellsOfConnection);
        conduitVisuals.Add(connectionId, conduitVisual);
    }


    private ConduitVisual CreateItem()
    {
        var conduitVisual = Instantiate(prefab);
        return conduitVisual.GetComponent<ConduitVisual>();
    }

    private void OnGet(ConduitVisual conduitVisual)
    {
        conduitVisual.Reset();
        conduitVisual.conduitVisualizer = this;
        conduitVisual.gameObject.SetActive(true);
    }


    private void OnRelease(ConduitVisual conduitVisual)
    {
        conduitVisual.gameObject.SetActive(false);
    }

    // Called when the pool decides to destroy an item (e.g., above max size).
    private void OnDestroyItem(ConduitVisual conduitVisual)
    {
        Destroy(conduitVisual.gameObject);
    }

    public void ReleaseItem(ConduitVisual conduitVisual)
    {
        conduitVisual.Reset();
        pool.Release(conduitVisual);
    }

    public void OnDeleteConduit(GUID guid)
    {
        if (!conduitVisuals.ContainsKey(guid)) return;
        ConduitVisual conduitVisual = conduitVisuals[guid];
        ReleaseItem(conduitVisual);
        conduitVisuals.Remove(guid);
    }

    public void StartDrag(NodeVisual nodeVisual)
    {
        if (previewConduitVisual) return; // Already dragging
        previewConduitVisual = pool.Get();
        previewConduitVisual.StartNewConduitAtNode(nodeVisual,
            GameFrontendManager.Instance.temporalLayerStack.GetLayerByNum(nodeVisual.layerNum));
    }

    // Resets the drag state
    public void CancelDrag()
    {
        if (!previewConduitVisual) return;

        RaycastHit[] hits = cameraController.RaycastAll();
        foreach (RaycastHit hit in hits)
        {
            NodeVisual endNodeVisual = hit.collider.GetComponent<NodeVisual>();
            if (endNodeVisual)
            {
                CompleteConduit(endNodeVisual);
                return;
            }
        }

        ReleaseItem(previewConduitVisual);
        previewConduitVisual = null;
    }

    private void CompleteConduit(NodeVisual endNodeVisual)
    {
        previewConduitVisual.planeB =
            GameFrontendManager.Instance.temporalLayerStack.GetLayerByNum(endNodeVisual.layerNum);
        GUID? conduitBackendID =
            GameFrontendManager.Instance.IsValidConduit(previewConduitVisual.sourceNodeVisual, endNodeVisual,
                previewConduitVisual.GetCellsOfConnection(),previewConduitVisual.bridgesBuilt);
        if (conduitBackendID != null)
        {
            if (previewConduitVisual.FinalizeConduit(endNodeVisual, conduitBackendID.Value))
            {
                conduitVisuals.Add(conduitBackendID.Value, previewConduitVisual);
                previewConduitVisual = null;
                return;
            }
        }

        //else invalid conduit, cancel 
        ReleaseItem(previewConduitVisual);
        previewConduitVisual = null;
    }

    void Update()
    {
        // --- While dragging: update temp line and check for mouse up to finish ---
        if (previewConduitVisual)
        {
            RaycastHit raycastHit;
            if (cameraController.RaycastForFirst(out raycastHit))
            {
                Vector3 lineEnd = raycastHit.point;
                GameObject hitObject = raycastHit.collider.gameObject;

                CoordinatePlane frame = hitObject.GetComponentInParent<CoordinatePlane>();
                if (frame) previewConduitVisual.SetPreviewPosition(lineEnd, frame);
            }
        }
    }

    public ConduitVisual GetConduitVisual(GUID guid)
    {
        if (conduitVisuals.ContainsKey(guid))
        {
            return conduitVisuals[guid];
        }

        return null;
    }
}