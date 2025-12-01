using System.Collections.Generic;
using NodeBase;
using UnityEditor;
using UnityEngine;

namespace Interfaces
{
    public interface IFrontend
    {
        // void UpdateEnergyPackets( List<EnergyPacket> energyPackets);
        // void DeleteEnergyPackets( List<EnergyPacket> energyPackets); each packet asks for its pos isntead to make use of pooling
        void GameOver(string reason);
        bool PlaceNodeVisual(AbstractNodeInstance node, int layerNum, Vector2 planePos);
        
        void SpawnEnergyPacket(GUID guid);
        void DeleteEnergyPacket(GUID guid);

        bool AddTimeSlice(int sliceNum);
        //ui healthbar etc later
    }
}