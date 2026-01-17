using System;
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
                Debug.Log("Could not spawn time ripple in T-1. So we try to place an anti node");
                if (slice.TimeSliceGrid.IsCellOccupied(_pos, out var node, out var connection))
                {
                    if (node != null)
                    {
                        slice.spawnBlackHole(_pos, out var newBlackHole);
                        if (newBlackHole == null)
                        {
                            Debug.Log("Error white spawning black hole!");
                            return;
                        }
                        storage.Frontend.PlaceNodeVisual(newBlackHole.guid, newBlackHole.NodeType.NodeDTO, slice.SliceNumber, new Vector2(_pos.x, _pos.y), _energyType);
                        Debug.Log("Placed a black hole!");
                    }
                    else if (connection != null)
                    {
                        if (connection.Count > 1)
                        {
                            Debug.LogWarning("Cannot spawn Blockade because connection is a bridge with 2 connections in the cell at "+_pos+" in slice "+slice.SliceNumber);
                            return;
                        }
                        slice.spawnBlockadeInConnection(connection.First(), _pos, out var newBlockade);
                        if (newBlockade == null)
                        {
                            Debug.Log("\"Error while spawning Blockade!");
                            return;
                        }

                        Debug.Log("Placed a blockade!");
                    }
                }
                else
                {
                    throw new Exception(
                        "Cell not occupied so we won't place an anti node. This is a bug! We were not able to place then node in T-" +
                        slice.SliceNumber);
                }
            }
        }
    }
}