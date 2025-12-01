using System;
using Interfaces;
using NodeBase;
using UnityEditor;
using UnityEngine;

public class EnergyPacketVisual : MonoBehaviour
{
    public ConduitVisual conduit;
    public GUID guid{get; set;}
    //private readonly float speed = 0.2f;
    private float progress;
public IBackend backend; //TODO giga dirty. fix backend reference later
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
    }


    private void LateUpdate()
    {
        AbstractNodeInstance sourceNode;
        AbstractNodeInstance targetNode;
        progress = backend.GetEnergyPacketProgress(guid,out sourceNode, out targetNode);
        
        //lerp between conduit.nodeA and conduit.nodeB based on progress
            if (progress >= 1f) EnergyPacketVisualizer.Instance.ReleaseItem(this);
            var startPos = sourceNode.Pos;
            var endPos = targetNode.Pos;
            transform.position = Vector3.Lerp(startPos, endPos, progress);
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