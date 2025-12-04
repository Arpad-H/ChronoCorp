using System;
using Interfaces;
using NodeBase;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public class EnergyPacketVisual : MonoBehaviour
{
    public ConduitVisual conduit;

    public GUID guid;
    private EnergyType energyType;
    public String debugInfo;
    public String debugconduitID;
    //private readonly float speed = 0.2f;
   

    public IBackend backend; //TODO giga dirty. fix backend reference later

    public SpriteRenderer sprite;
    public SplineAnimate splineAnimate;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (!sprite) sprite = GetComponent<SpriteRenderer>();
        if (!splineAnimate) splineAnimate = GetComponent<SplineAnimate>();
    }

    public void SetEnergyType(EnergyType newEnergyType)
    {
        Color color = newEnergyType.ToColor();
        energyType = newEnergyType;
        sprite.color = color;
    }

    private void LateUpdate()
    {
        debugInfo = guid.ToString();
        if (backend == null)
        {
            return;
        }
        Vector3? sourceNode;
        Vector3? targetNode;
        GUID? conduitID;
        float progress = backend.GetEnergyPacketProgress(guid, out sourceNode, out targetNode, out conduitID);
        if (conduitID.HasValue)
        {
            conduit = ConduitVisualizer.Instance.GetConduitVisual(conduitID.Value);
            splineAnimate.Container = conduit.splineContainer;
            debugconduitID = conduit.backendID.ToString();
        }
        
        // determine direction
        if (sourceNode.HasValue && targetNode.HasValue)
        {
            splineAnimate.Container = conduit.splineContainer;
            Vector3 startPos = sourceNode.Value;
            Spline spline = conduit.splineContainer.Splines[0];
            Vector3 splineStartPos = spline[0].Position;
            Vector3 splineEndPos = spline[spline.Count - 1].Position;
            Vector3 endPos = splineEndPos - startPos;
            
            if (Vector3.Distance(startPos, splineStartPos) < 0.1f)
            {
              progress = 1- progress; // reverse direction
            }
            else if (Vector3.Distance(splineEndPos, endPos) < 0.1f)
            {
                progress =  progress; // normal direction
            }
        }
        splineAnimate.NormalizedTime = progress;
        
        
       
        // if (sourceNode == null || targetNode == null) return;
        // if (Mathf.Approximately(progress, -1))
        // {
        //     return;
        // }
        //
        // //lerp between conduit.nodeA and conduit.nodeB based on progress
        // Vector2 startPos, endPos = Vector2.zero;
        // startPos.x = sourceNode.Value.x;
        // startPos.y = sourceNode.Value.y;
        // endPos.x = targetNode.Value.x;
        // endPos.y = targetNode.Value.y;
        // int startPosLayer = (int)sourceNode.Value.z;
        // int endPosLayer = (int)targetNode.Value.z;
        // CoordinatePlane startPlane = GameFrontendManager.Instance.GetCoordinatePlane(startPosLayer);
        // CoordinatePlane endPlane = GameFrontendManager.Instance.GetCoordinatePlane(endPosLayer);
        // Vector3 worldStartPos = startPlane.GridToWorldPosition(startPos);
        // Vector3 worldEndPos = endPlane.GridToWorldPosition(endPos);
        // transform.position =
        //     Vector3.Lerp(worldStartPos, worldEndPos, progress) + new Vector3(0, 0.1f, 0); // Slightly in front
    }

    public void Reset()
    {
        conduit = null;
        energyType = EnergyType.WHITE;
    }
}