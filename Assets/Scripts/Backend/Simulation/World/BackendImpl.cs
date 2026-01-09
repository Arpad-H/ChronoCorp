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
                    foreach (var generatorInstanceTotalOutput in generatorInstance.totalOutputs)
                    {
                        UnlinkNodes(generatorInstanceTotalOutput.Connection.guid, false);
                    }
                    _storage.recalculatePaths();
                    return timeSlice?.removeNode(nodeBackendId) ?? false;
                }
                default:
                    return false;
            }
        }

        public GUID? LinkNodes(GUID backendIdA, GUID backendIdB, Vector2[] cellsOfConnection)
        {
            var connectionID = _storage.link(backendIdA, backendIdB);
            if (connectionID != null) _storage.recalculatePaths();
            return connectionID;
        }

        public bool UnlinkNodes(GUID connectionId)
        {
            return UnlinkNodes(connectionId, true);
        }

        public bool UnlinkNodes(GUID connectionId, bool recalculatePaths)
        {
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

        public int getAmountPlaceable(NodeDTO nodeDTO)
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