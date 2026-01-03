using Backend.Simulation.World;
using NodeBase;
using UnityEditor;
using UnityEngine;

namespace Interfaces
{
    public interface IFrontend
    {
       
        void GameOver(string reason);
        
        /**
         * Tells the frontend about a node that was spawned by the backend at backend cellPos.
         */
        bool PlaceNodeVisual(GUID id,NodeDTO nodeDto, int layerNum, Vector2 cellPos, EnergyType energyType);
        
        /**
         * Tells the frontend to spawn an energy packet visualizer for the packet with the given GUID and energy type.
         */
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

        void onNodeHealthChange(GUID id, int minValue, int maxValue, int currentValue);
        //ui healthbar etc later
    }
}