using System.Collections.Generic;
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
            Vector2Int[] cellsOfConnection,
            int bridgesBuilt)
        {
            var sliceA = getTimeSliceOfNodeByGuid(a);
            var sliceB = getTimeSliceOfNodeByGuid(b);

            if (sliceA == null || sliceB == null)
                return null;

            var grid = sliceA.TimeSliceGrid;
            int numOfConnectionsCrossed = 0;
            foreach (var cell in cellsOfConnection)
            {
                if (grid.IsCellOccupied(cell, out var node, out var connection))
                {
                    if (node != null && (!node.guid.Equals(a) && !node.guid.Equals(b)))
                    {
                        Debug.Log("Cannot link because there is a node in its path in slices: "+sliceA.SliceNumber+"-"+sliceB.SliceNumber);
                        return null;
                    }

                    if (connection != null)
                    {
                        numOfConnectionsCrossed++;
                        if (numOfConnectionsCrossed > bridgesBuilt)
                        {
                            Debug.Log("Cannot link because there are not enough bridges built to cross connections in slices: "+sliceA.SliceNumber+"-"+sliceB.SliceNumber);
                            return null;
                        }
                    }
                }
            }

            var connectionId = _storage.link(a, b, cellsOfConnection);
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
                _storage.guidToNodesMapping[b],
                bridgesBuilt
                );

            if (!reserved)
            {
                _storage.unlink((GUID)connectionId);
                return null;
            }
            _storage.recalculatePaths();
            return connectionObj.guid;
        }

        public bool upgradeGenerator(GUID generatorGUID)
        {
            _storage.guidToNodesMapping.TryGetValue(generatorGUID, out var foundNode);
            if (foundNode != null && foundNode is GeneratorInstance generatorInstance && generatorInstance.totalOutputs.Count < 4)
            {
                generatorInstance.totalOutputs.Add(new Output());
                return true;
            }
            return false;
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

        public float GetEnergyPacketProgress(GUID packet, out GUID? sourcePos, out GUID? targetPos,
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

                sourcePos = sourceNode.guid;
                targetPos = targetNode.guid;
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

        public int GetAmountPlaceable(InventoryItem item)
        {
            return _storage.inventory.getAmountPlaceable(item);
        }
        public int AddItemToInventory (InventoryItem item, int amount)
        {
            return _storage.inventory.addItem(item, amount);
        }

        public bool IsConnectionPathOccupied(int layerNum, Vector2Int[] cellsOfConnection)
        {
            var timeSlice = byLayerNum(layerNum);
            var grid = timeSlice.TimeSliceGrid;
            
            for (int i = 1; i < cellsOfConnection.Length; i++) 
            {
                var cell = cellsOfConnection[i];
                if (grid.IsCellOccupied(cell, out var node, out var connection))
                {
                    return true;
                }
            }

            return false;
        }

        private TimeSlice byLayerNum(int layerNum)
        {
            return _storage.timeSlices[layerNum];
        }

        [CanBeNull]
        private TimeSlice getTimeSliceOfNodeByGuid(GUID guid)
        {
            _storage.guidToNodesMapping.TryGetValue(guid, out var node);
            if (node != null)
            {
                return node.currentTimeSlice;
            }

            return null;
        }

        // public void UpgradeCardSelected(UpgradeData upgradeData)
        // {
        //     throw new System.NotImplementedException();
        // }
    }
}