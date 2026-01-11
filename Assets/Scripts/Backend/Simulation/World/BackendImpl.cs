using System.Linq;
using Interfaces;
using JetBrains.Annotations;
using NodeBase;
using UnityEditor;
using UnityEngine;

namespace Backend.Simulation.World
{
    public class BackendImpl : IBackend
    {
        private readonly SimulationStorage _storage;
        public readonly IFrontend FrontendCallback;

        public BackendImpl(IFrontend frontend)
        {
            FrontendCallback = frontend;
            _storage = new SimulationStorage(FrontendCallback);
            _storage.onPacketDeleted += guid => FrontendCallback.DeleteEnergyPacket(guid);
        }

        public GUID? PlaceNode(NodeDTO nodeType, int LayerNum, Vector2 planePos, EnergyType? et)
        {
            var timeSlice = byLayerNum(LayerNum);

            GUID? guid = null;
            var energyType = et ?? EnergyType.WHITE;

            if (nodeType.Equals(NodeDTO.GENERATOR)) guid = timeSlice.spawnGenerator(planePos, 1);
            if (nodeType.Equals(NodeDTO.RIPPLE))
            {
                guid = timeSlice.spawnRipple(planePos, energyType, out var newTimeRippleInstance);
                if (newTimeRippleInstance != null) energyType = newTimeRippleInstance.EnergyType;
            }

            return guid;
        }

        public bool DeleteNode(GUID nodeBackendId)
        {
            var timeSlice = getTimeSliceOfNodeByGuid(nodeBackendId);
            _storage.guidToNodesMapping.TryGetValue(nodeBackendId, out var foundNode);
            switch (foundNode)
            {
                case null:
                    return false;
                case GeneratorInstance generatorInstance:
                {
                    foreach (Output generatorInstanceTotalOutput in generatorInstance.totalOutputs)
                    {
                        if (generatorInstanceTotalOutput != null && generatorInstanceTotalOutput.Connection != null) //TODO did this as a quick fix when deleting a generator with no connecections
                        {
                            UnlinkNodes(generatorInstanceTotalOutput.Connection.guid, false);
                        }
                        
                    }
                    _storage.recalculatePaths();
                    return timeSlice?.removeNode(nodeBackendId) ?? false;
                }
                default:
                    return false;
            }
        }
        
        public GUID? LinkNodes(
            GUID a,
            GUID b,
            Vector2[] cellsOfConnection)
        {
            var sliceA = getTimeSliceOfNodeByGuid(a);
            var sliceB = getTimeSliceOfNodeByGuid(b);

            if (sliceA == null || sliceA != sliceB)
                return null;

            var grid = sliceA.TimeSliceGrid;

            foreach (var cell in cellsOfConnection)
            {
                if (grid.IsCellOccupied(cell, out var node, out var connection))
                {
                    if (node != null && (!node.guid.Equals(a) && !node.guid.Equals(b)))
                    {
                        Debug.Log("Cannot link because there is a node in its path");
                        return null;
                    }

                    if (connection != null)
                    {
                        Debug.Log("Cannot link because there is a connection in its path");
                        return null;
                    }
                }
            }

            var connectionId = _storage.link(a, b);
            if (connectionId == null)
            {
                Debug.Log("Connection could not be created!");
                return null;
            }
            Debug.Log("Linked nodes -> cells: "+string.Join(",", cellsOfConnection.Select(x => x.ToString()).ToArray()));

            var connectionObj = _storage.guidToConnections[(GUID)connectionId];

            bool reserved = grid.TryAddConnectionCells(
                connectionObj,
                cellsOfConnection,
                _storage.guidToNodesMapping[a],
                _storage.guidToNodesMapping[b]
                );

            if (!reserved)
            {
                _storage.unlink((GUID)connectionId);
                return null;
            }
            _storage.recalculatePaths();
            return connectionObj.guid;
        }

        public bool UnlinkNodes(GUID connectionId)
        {
            return UnlinkNodes(connectionId, true);
        }

        public bool UnlinkNodes(GUID connectionId, bool recalculatePaths)
        {
            foreach (var slice in _storage.timeSlices)
            {
                slice.TimeSliceGrid.RemoveConnectionCells(connectionId);
            }

            if (_storage.unlink(connectionId))
            {
                if(recalculatePaths) _storage.recalculatePaths();
                return true;
            }

            return false;
        }

        public float GetEnergyPacketProgress(GUID packet, out Vector3? sourcePos, out Vector3? targetPos,
            out GUID? connectionID)
        {
            if (!_storage.energyPackets.ContainsKey(packet))
            {
                sourcePos = null;
                targetPos = null;
                connectionID = null;
                return -1;
            }

            var foundPacket = _storage.energyPackets[packet];
            if (foundPacket != null)
            {
                var sourceNode = foundPacket.currentStep().getStart();
                var targetNode = foundPacket.currentStep().getEnd();

                var timeSliceSource = getTimeSliceOfNodeByGuid(sourceNode.guid);
                var timeSliceTarget = getTimeSliceOfNodeByGuid(targetNode.guid);

                // Only happens if start / end was deleted from the simulation
                if (timeSliceSource == null || timeSliceTarget == null)
                {
                    sourcePos = null;
                    targetPos = null;
                    connectionID = null;
                    return -1;
                }

                sourcePos = new Vector3(sourceNode.Pos.x, sourceNode.Pos.y, timeSliceSource.SliceNumber);
                targetPos = new Vector3(targetNode.Pos.x, targetNode.Pos.y, timeSliceTarget.SliceNumber);
                connectionID = foundPacket.currentStep().connection.guid;
                return foundPacket.progressOnEdge;
            }

            sourcePos = null;
            targetPos = null;
            connectionID = null;
            return -1;
        }

        public bool getValuesForStabilityMalusType(StabilityMalusType type, out int threshold)
        {
            threshold = _storage.StabilityBar.getActivationThreshold(type);
            return _storage.StabilityBar.IsMalusActiveByType(type);
        }
        // public GUID? GetRippleConnectionOfEnergyPacket(GUID packet)
        // {
        //     var foundPacket = _storage.energyPackets[packet];
        //     if (foundPacket != null)
        //     {
        //    
        //         return foundPacket.currentStep().connection.guid;
        //     }
        //     return null;
        // }

        public void tick(long tickCount, IFrontend frontend)
        {
            _storage.tick(tickCount, frontend);
        }

        public int GetAmountPlaceable(NodeDTO nodeDTO)
        {
            return _storage.inventory.getAmountPlaceable(nodeDTO);
        }

        private TimeSlice byLayerNum(int layerNum)
        {
            return _storage.timeSlices[layerNum];
        }

        [CanBeNull]
        private TimeSlice getTimeSliceOfNodeByGuid(GUID guid)
        {
            foreach (var timeSlice in _storage.timeSlices)
                if (timeSlice.isNodeKnown(guid))
                    return timeSlice;

            return null;
        }
    }
}