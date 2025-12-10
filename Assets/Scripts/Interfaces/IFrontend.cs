using Backend.Simulation.World;
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
        
        /**
         * Tells the frontend about a node that was spawned by the backend at backend cellPos.
         */
        bool PlaceNodeVisual(NodeDTO nodeDto, int layerNum, Vector2 cellPos, EnergyType energyType);

        void SpawnEnergyPacket(GUID guid, EnergyType energyType);
        void DeleteEnergyPacket(GUID guid);

        /**
         * Called by the backend whenever the value of the stability bar changes. Can be used to change UI accordingly
         */
        void OnStabilityBarUpdate(int minValue, int maxValue, int currentValue);

        /**
         * Called when a stability debuff effect is activated due to low stability
         */
        void OnActivateStabilityMalus(StabilityMalusType stabilityMalusType);

        /**
        * Called when a stability debuff effect is deactivated due to high stability
        */
        void OnDeactivateStabilityMalus(StabilityMalusType stabilityMalusType);

        bool AddTimeSlice(int sliceNum);
        //ui healthbar etc later
    }
}