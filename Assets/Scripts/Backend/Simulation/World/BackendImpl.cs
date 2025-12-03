using Backend.Simulation.Energy;
using Interfaces;
using JetBrains.Annotations;
using NodeBase;
using UnityEditor;
using UnityEngine;

namespace Backend.Simulation.World
{
    public class BackendImpl : IBackend
    {
        public readonly IFrontend FrontendCallback;
        private SimulationStorage _storage;

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
            EnergyType energyType =  et ?? EnergyType.WHITE;

            if (nodeType.Equals(NodeDTO.GENERATOR)) guid = timeSlice.spawnGenerator(planePos, 1);
            if (nodeType.Equals(NodeDTO.RIPPLE))
            {
                guid = timeSlice.spawnRipple(planePos,energyType);
                if(guid != null) energyType = ((TimeRippleInstance)_storage.guidToNodesMapping[(GUID) guid]).EnergyType;
            }

            return guid;
        }

        public bool DeleteNode(GUID nodeBackendId)
        {
            var timeSlice = byObjectGuid(nodeBackendId);
            return timeSlice?.removeNode(nodeBackendId) ?? false;
        }

        public bool LinkNodes(GUID backendIdA, GUID backendIdB, out GUID? connectionID)
        {
            connectionID = _storage.link(backendIdA, backendIdB);
            if (connectionID != null)
            {
                _storage.recalculatePaths((GUID)connectionID);
            }
            return connectionID != null;
        }

        public bool UnlinkNodes(GUID connectionId)
        {
            if (_storage.unlink(connectionId))
            {
                _storage.recalculatePaths(connectionId);
                return true;
            }

            return false;
        }

        public float GetEnergyPacketProgress(GUID packet, out Vector3? sourcePos, out Vector3? targetPos) //TODO had to change it to know the source adn target
        {
            if (!_storage.energyPackets.ContainsKey(packet))
            {
                sourcePos = null;
                targetPos = null;
                
                return -1;
            }
            var foundPacket = _storage.energyPackets[packet];
            if (foundPacket != null)
            {
                var sourceNode = foundPacket.currentStep().getStart();
                var targetNode = foundPacket.currentStep().getEnd();

                var timeSliceSource = byObjectGuid(sourceNode.guid);
                var timeSliceTarget = byObjectGuid(targetNode.guid);

                sourcePos = new Vector3(sourceNode.Pos.x, sourceNode.Pos.y, timeSliceSource.SliceNumber);
                targetPos = new Vector3(targetNode.Pos.x, targetNode.Pos.y, timeSliceTarget.SliceNumber);
                return foundPacket.progressOnEdge;
            }

            sourcePos = null;
            targetPos = null;
            return -1;
        }
        // public float GetEnergyPacketProgress(GUID packet, out GUID? connectionID)
//         {
//
//             var foundPacket = _storage.energyPackets[packet];
//             if (foundPacket != null)
//             {
//                 connectionID = foundPacket.currentStep().connection.guid;
//                 return foundPacket.progressOnEdge;
//             }
//
//             connectionID = null;
//             return -1;
//         }

        public void tick(long tickCount, IFrontend frontend)
        {
            _storage.tick(tickCount, frontend);
        }

        private TimeSlice byLayerNum(int layerNum)
        {
            return _storage.timeSlices[layerNum];
        }

        [CanBeNull]
        private TimeSlice byObjectGuid(GUID guid)
        {
            foreach (var timeSlice in _storage.timeSlices)
                if (timeSlice.isNodeKnown(guid))
                    return timeSlice;

            return null;
        }
    }
}