using System.Linq;
using Backend.Simulation.World;
using NodeBase;
using UnityEngine;

namespace Backend.Simulation.SimEvent
{
    public sealed class SpawnTimeRippleEvent : ISimEvent
    {
        public string Name => "SpawnTimeRipple";

        private readonly Vector2 _pos;
        private readonly EnergyType _energyType;

        public SpawnTimeRippleEvent(Vector2 pos, EnergyType energyType)
        {
            _pos = pos;
            _energyType = energyType;
        }

        public void Execute(SimulationStorage storage, TimeSlice slice, long globalTick)
        {
            Debug.Log("Spawning time ripple in past time slice: "+slice.SliceNumber);
            slice.spawnRipple(_pos, _energyType, out var newTimeRipple);
            if(newTimeRipple != null)
            {
                storage.Frontend.PlaceNodeVisual(newTimeRipple.guid, newTimeRipple.NodeType.NodeDTO, slice.SliceNumber, new Vector2(_pos.x, _pos.y), _energyType);
            }
            else
            {
                if (slice.TimeSliceGrid.IsCellOccupied(_pos, out var node, out var connection))
                {
                    if (node != null)
                    {
                        slice.spawnBlackHole(_pos, out var newBlackHole);
                        storage.Frontend.PlaceNodeVisual(newBlackHole.guid, newBlackHole.NodeType.NodeDTO, slice.SliceNumber, new Vector2(_pos.x, _pos.y), _energyType);
                    }
                    else if (connection != null)
                    {
                        if (connection.Count > 1)
                        {
                            Debug.LogWarning("Cannot spawn Blockade because connection is a bridge with 2 connections in the cell at "+_pos+" in slice "+slice.SliceNumber);
                            return;
                        }
                        slice.spawnBlockadeInConnection(connection.First(), _pos, out var newBlockade);
                        storage.Frontend.PlaceNodeVisual(newBlockade.guid, newBlockade.NodeType.NodeDTO, slice.SliceNumber, new Vector2(_pos.x, _pos.y), _energyType);
                    }
                }
            }
        }
    }
}