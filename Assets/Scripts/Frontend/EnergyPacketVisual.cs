using System;
using Interfaces;
using NodeBase;
using UnityEditor;
using UnityEngine;

public class EnergyPacketVisual : MonoBehaviour
{
    public ConduitVisual conduit;

    public GUID guid { get; set; }

    //private readonly float speed = 0.2f;
    public float progress;

    public IBackend backend; //TODO giga dirty. fix backend reference later

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
    }


    private void LateUpdate()
    {
        if (backend == null)
        {
            return;
        }
        Vector3? sourceNode;
        Vector3? targetNode;
        progress = backend.GetEnergyPacketProgress(guid, out sourceNode, out targetNode);
        if (sourceNode == null || targetNode == null) return;
        if (Mathf.Approximately(progress, -1))
        {
            return;
        }
        
        //lerp between conduit.nodeA and conduit.nodeB based on progress
        if (progress >= 1f) EnergyPacketVisualizer.Instance.ReleaseItem(this);
        Vector2 startPos, endPos = Vector2.zero;
        startPos.x = sourceNode.Value.x;
        startPos.y = sourceNode.Value.y;
        endPos.x = targetNode.Value.x;
        endPos.y = targetNode.Value.y;
        int startPosLayer = (int)sourceNode.Value.z;
        int endPosLayer = (int)targetNode.Value.z;
        CoordinatePlane startPlane = GameFrontendManager.Instance.GetCoordinatePlane( startPosLayer);
        CoordinatePlane endPlane = GameFrontendManager.Instance.GetCoordinatePlane( endPosLayer);
        Vector3 worldStartPos = startPlane.GridToWorldPosition(startPos);
        Vector3 worldEndPos = endPlane.GridToWorldPosition(endPos);
       transform.position = Vector3.Lerp(worldStartPos, worldEndPos, progress) + new Vector3(0,0, -0.2f); // Slightly in front
    }

    public void SetConduit(ConduitVisual conduitVisual)
    {
        this.conduit = conduitVisual;
        progress = 0f; //reset progress
    }

    public void Reset()
    {
        conduit = null;
        backend = null;
        progress = 0f;
    }
}