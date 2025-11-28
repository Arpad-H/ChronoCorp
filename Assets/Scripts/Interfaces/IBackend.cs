using Lukas.Simulation.Energy;
using NUnit.Framework;
using UnityEngine;
namespace Interfaces
{
    public interface IBackend
    {
        
       //Converts plane coords (0→width-1, 0→height-1) into the nodeContainer local space
       bool PlaceNode(GameObject prefab,int LayerNum, Vector2 planePos);
       bool DeleteNode(GameObject nodeObj);
       bool LinkNodes(GameObject nodeA, GameObject nodeB, out GameObject conduitObj);
       bool UnlinkNodes(GameObject conduitObj);
       // TimeLayer GetTimeLayer(int layerNum); //change time layer to whatever applies
      
       
    }
}