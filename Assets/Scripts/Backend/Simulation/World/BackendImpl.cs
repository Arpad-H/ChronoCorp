using System;
using System.Linq;
using Interfaces;
using JetBrains.Annotations;
using NodeBase;
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

        public Guid? PlaceNode(NodeDTO nodeType, int LayerNum, Vector2 planePos, EnergyType? et)
        {
            var timeSlice = byLayerNum(LayerNum);

            Guid? guid = null;
            var energyType = et ?? EnergyType.WHITE;

            if (nodeType.Equals(NodeDTO.GENERATOR)) guid = timeSlice.spawnGenerator(planePos, 1);
            if (nodeType.Equals(NodeDTO.RIPPLE))
            {
                guid = timeSlice.spawnRipple(planePos, energyType, out var newTimeRippleInstance);
                if (newTimeRippleInstance != null) energyType = newTimeRippleInstance.EnergyType;
            }

            return guid;
        }

        public bool DeleteNode(Guid nodeBackendId)
        {
            return _storage.deleteNode(nodeBackendId);
        }
        
        public Guid? LinkNodes(
            Guid a,
            Guid b,
            Vector2Int[] cellsOfConnection,
            int bridgesBuilt
            )
        {
            return _storage.LinkNodes(a, b, cellsOfConnection, bridgesBuilt);
        }

        public bool upgradeGenerator(Guid generatorGUID)
        {
            _storage.guidToNodesMapping.TryGetValue(generatorGUID, out var foundNode);
            if (foundNode != null && foundNode is GeneratorInstance generatorInstance && generatorInstance.totalOutputs.Count < 4)
            {
                generatorInstance.totalOutputs.Add(new Output());
                return true;
            }
            return false;
        }

        public bool UnlinkNodes(Guid connectionId)
        {
            return UnlinkNodes(connectionId, true);
        }

        public bool UnlinkNodes(Guid connectionId, bool recalculatePaths)
        {
            return _storage.UnlinkNodes(connectionId, recalculatePaths);
        }

        public float GetEnergyPacketProgress(Guid packet, out Guid? sourcePos, out Guid? targetPos,
            out Guid? connectionID)
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
        private TimeSlice getTimeSliceOfNodeByGuid(Guid guid)
        {
            return _storage.getTimeSliceOfNodeByGuid(guid);
        }

        // public void UpgradeCardSelected(UpgradeData upgradeData)
        // {
        //     throw new System.NotImplementedException();
        // }
    }
}