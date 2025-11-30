using System.Collections.Generic;
using Backend.Simulation.Energy;
using Interfaces;
using NodeBase;
using UnityEditor;
using UnityEngine;

namespace Backend.Simulation.World
{
    public class SimulationStorage
    {
        private readonly IFrontend _frontendCallback;
        public Dictionary<GUID, Connection> guidToConnections = new();
        public Dictionary<GUID, AbstractNodeInstance> guidToNodesMapping = new();
        public Dictionary<GUID, EnergyPacket> energyPackets = new();
        public readonly List<TimeSlice> timeSlices = new();
        
        public SimulationStorage(IFrontend frontendCallback)
        {
            _frontendCallback = frontendCallback;
            timeSlices[0] = new TimeSlice(this);
        }

        public bool isNodeKnown(GUID guid)
        {
            return guidToNodesMapping.ContainsKey(guid);
        }

        public void registerEnergyPacket(EnergyPacket energyPacket)
        {
            energyPackets[energyPacket.Guid] = energyPacket;
            _frontendCallback.SpawnEnergyPacket(energyPacket.Guid);
            Debug.Log("Created new energy packet "+energyPacket.Guid);
        }

        public void tick(long tickCount)
        {
            foreach (var timeSlice in timeSlices)
            {
                timeSlice.tick(tickCount);
            }

            foreach (var packet in energyPackets.Values)
            {
                packet.tick(tickCount);
                if (packet.Delivered)
                {
                    //TODO: Call packet delivered event for frontend -> Removes packet in frontend
                    //TODO: Consume packet on destination node
                    energyPackets.Remove(packet.Guid);
                    Debug.Log("Energy packet "+packet.Guid+" delivered.");
                }
            }
        }
        
        public GUID? link(GUID idNode1, GUID idNode2)
        {
            if (!isNodeKnown(idNode1) || !isNodeKnown(idNode2))
            {
                return null;
            }

            var node1 = guidToNodesMapping[idNode1];
            var node2 = guidToNodesMapping[idNode2];

            if (node1 is GeneratorInstance generator1 && node2 is GeneratorInstance generator2)
            {
                return null;
            }
            if (node1 is NodeWithConnections ripple1 && node2 is NodeWithConnections ripple2)
            {
                
                if (ripple1.HasDirectConnectionTo(ripple2 as AbstractNodeInstance))
                {
                    return null;
                }
                
                var rippleConnection = new Connection(ripple1 as AbstractNodeInstance, ripple2 as AbstractNodeInstance);
                ripple1.getConnections().Add(rippleConnection);
                ripple2.getConnections().Add(rippleConnection);
                guidToConnections[rippleConnection.guid] = rippleConnection;
                Debug.Log("Linked node "+node1.guid+" and "+node2.guid);
                return rippleConnection.guid;
            }

            if (node1 is GeneratorInstance gen1 && node2 is NodeWithConnections anyNode2)
            {
                return linkGeneratorAndNonGenerator(gen1, anyNode2);
            }
            
            if (node2 is GeneratorInstance gen2 && node1 is NodeWithConnections anyNode1)
            {
                return linkGeneratorAndNonGenerator(gen2, anyNode1);
            }

            GUID? linkGeneratorAndNonGenerator(GeneratorInstance generator, NodeWithConnections anyNode)
            {
                if (generator.alreadyConnectedTo(anyNode as AbstractNodeInstance))
                {
                    return null;
                }
                var foundOutput = generator.findFreeOutput();
                if (foundOutput == null)
                {
                    return null;
                }
                var rippleConnection = new Connection(node1, node2);
                foundOutput.Connection = rippleConnection;
                anyNode.getConnections().Add(rippleConnection);
                guidToConnections[rippleConnection.guid] = rippleConnection;
                Debug.Log("Linked generator "+generator.guid+" to node "+(anyNode as AbstractNodeInstance).guid);
                return rippleConnection.guid;
            }

            return null;
        }

        public bool unlink(GUID connectionId)
        {
            var foundConnection = guidToConnections[connectionId];
            if (foundConnection == null) return false;

            var node1 = foundConnection.node1;
            var node2 = foundConnection.node2;
            unlinkFromConnection(node1, foundConnection);
            unlinkFromConnection(node2, foundConnection);
            Debug.Log("Unlinked "+node1.guid+" and "+node2.guid);
            return true;

            void unlinkFromConnection(AbstractNodeInstance anyNode, Connection connection)
            {
                if (anyNode is GeneratorInstance generator)
                {
                    var foundOutput = generator.findOutputWithConnection(connection);
                    if (foundOutput != null) foundOutput.Connection = null;
                }

                if (anyNode is NodeWithConnections anyNodeWithConnections)
                {
                    anyNodeWithConnections.getConnections().Remove(connection);
                }
            }
        }
    }

    public class TimeSlice
    {
        private const int TOLERANCE = 1;
        private readonly SpatialHashGrid spatialHashGrid = new(1);
        private readonly SimulationStorage _simulationStorage;

        public TimeSlice(SimulationStorage simulationStorage)
        {
            _simulationStorage = simulationStorage;
        }

        public GUID? spawnGenerator(Vector2 pos, int amountInitialOutputs)
        {
            if (spatialHashGrid.HasNodeNear(pos, TOLERANCE)) return null;
            var newNode = new GeneratorInstance(pos, amountInitialOutputs);
            spatialHashGrid.Add(newNode);
            _simulationStorage.guidToNodesMapping.Add(newNode.guid, newNode);
            Debug.Log("Created generator "+newNode.guid+" at "+pos);
            return newNode.guid;
        }

        public GUID? spawnRipple(Vector2 pos)
        {
            if (spatialHashGrid.HasNodeNear(pos, TOLERANCE)) return null;
            var newNode = new TimeRippleInstance(pos, random());
            spatialHashGrid.Add(newNode);
            _simulationStorage.guidToNodesMapping.Add(newNode.guid, newNode);
            Debug.Log("Created ripple "+newNode.guid+" at "+pos);
            return newNode.guid;
        }

        public bool removeNode(GUID guid)
        {
            if (_simulationStorage.guidToNodesMapping.TryGetValue(guid, out var nodeInstance))
                if (spatialHashGrid.Remove(nodeInstance))
                {
                    _simulationStorage.guidToNodesMapping.Remove(guid);
                    Debug.Log("Removed node "+guid);
                    return true;
                }

            return false;
        }

        public bool isNodeKnown(GUID guid)
        {
            return _simulationStorage.isNodeKnown(guid);
        }

        public void tick(long tickCount)
        {
            foreach (var abstractNodeInstance in _simulationStorage.guidToNodesMapping.Values)
            {
                abstractNodeInstance.tick(tickCount, _simulationStorage);
            }
        }

        private EnergyType random()
        {
            var values = (EnergyType[])EnergyType.GetValues(typeof(EnergyType));
            return values[Random.Range(0, values.Length)];
        }
    }

    public interface TimeBasedEvent
    {
        void processEvent(TimeSlice timeSlice);
    }

    public class SpawnGeneratorEvent : TimeBasedEvent
    {
        private int amountInitialOutputs;
        private Vector2 pos;

        public SpawnGeneratorEvent(Vector2 pos, int amountInitialOutputs)
        {
            this.pos = pos;
            this.amountInitialOutputs = amountInitialOutputs;
        }

        public void processEvent(TimeSlice timeSlice)
        {
        }
    }
}