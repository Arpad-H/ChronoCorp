using System;
using System.Collections.Generic;
using Lukas.Simulation.Energy;
using NodeBase;
using NUnit.Framework;
using UnityEngine;

namespace Interfaces
{
    public interface IFrontend
    {
        // void UpdateEnergyPackets( List<EnergyPacket> energyPackets);
        // void DeleteEnergyPackets( List<EnergyPacket> energyPackets); each packet asks for its pos isntead to make use of pooling
        void GameOver(String reason);
        bool PlaceNodeVisual(AbstractNodeInstance node, int layerNum, Vector2 planePos);
        bool AddTimeSlice(int sliceNum);
        //ui healthbar etc later
    }
}