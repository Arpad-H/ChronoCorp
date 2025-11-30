using Backend.Simulation.Energy;
using Interfaces;
using JetBrains.Annotations;
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
        }

        public GUID? PlaceNode(NodeDTO nodeType, int LayerNum, Vector2 planePos)
        {
            var timeSlice = byLayerNum(LayerNum);

            GUID? guid = null;

            if (nodeType.Equals(NodeDTO.GENERATOR)) guid = timeSlice.spawnGenerator(planePos, 1);
            if (nodeType.Equals(NodeDTO.RIPPLE)) guid = timeSlice.spawnRipple(planePos);

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
            return connectionID != null;
        }

        public bool UnlinkNodes(GUID connectionId)
        {
            return _storage.unlink(connectionId);
        }

        public float GetEnergyPacketProgress(GUID packet, out GUID? connectionID)
        {

            var foundPacket = _storage.energyPackets[packet];
            if (foundPacket != null)
            {
                connectionID = foundPacket.currentStep().connection.guid;
                return foundPacket.progressOnEdge;
            }

            connectionID = null;
            return -1;
        }

        public void tick(long tickCount)
        {
            _storage.tick(tickCount);
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