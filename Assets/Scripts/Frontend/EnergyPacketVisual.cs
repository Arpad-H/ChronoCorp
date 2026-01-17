using System;
using Interfaces;
using NodeBase;
using Unity.VisualScripting;
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
    
    
    public void SetEnergyType(EnergyType newEnergyType)
    {
        Color color = newEnergyType.ToColor();
        energyType = newEnergyType;
     
    }

    private void LateUpdate()
    {
        debugInfo = guid.ToString();
      
        GUID? sourceNode;
        GUID? targetNode;
        GUID? conduitID;
        float progress = GameFrontendManager.Instance.GetEnergyPacketProgress(guid, out sourceNode, out targetNode, out conduitID);

        if (conduitID.HasValue)
        {
            conduit = ConduitVisualizer.Instance.GetConduitVisual(conduitID.Value);
            if (conduit.sourceNodeVisual.backendID == sourceNode)  conduit.AddBulge(progress);
            else conduit.AddBulge(1 - progress);
           
        }
    }

    public void Reset()
    {
        RemoveConduitBulge();
        conduit = null;
        energyType = EnergyType.WHITE;
    }

    public void RemoveConduitBulge()
    {
        if (conduit)
        {
            conduit.RemoveBulge();
        }
    }
}