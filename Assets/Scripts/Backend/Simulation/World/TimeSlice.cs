using System;
using System.Collections.Generic;
using Backend.Simulation.Energy;
using Interfaces;
using NodeBase;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Backend.Simulation.World
{
    public class SimulationStorage
    {
        public event Action<GUID> onPacketDeleted;
        private readonly IFrontend _frontendCallback;
        public Dictionary<GUID, Connection> guidToConnections = new();
        public Dictionary<GUID, AbstractNodeInstance> guidToNodesMapping = new();
        public Dictionary<GUID, EnergyPacket> energyPackets = new();
        public readonly List<TimeSlice> timeSlices = new();
        private readonly List<GUID> _removeBuffer = new List<GUID>(128); //buffer to avoid modifying collection during iteration
        public SimulationStorage(IFrontend frontendCallback)
        {
            _frontendCallback = frontendCallback;
            //timeSlices[0] = new TimeSlice(this);
            timeSlices.Add( new TimeSlice(this, 0)); //prevents out of bounds since starts of with count 0
        }

        public bool isNodeKnown(GUID guid)
        {
            return guidToNodesMapping.ContainsKey(guid);
        }

        public void registerEnergyPacket(EnergyPacket energyPacket)
        {
            energyPackets[energyPacket.Guid] = energyPacket;
            _frontendCallback.SpawnEnergyPacket(energyPacket.Guid,energyPacket.EnergyType);
            Debug.Log("Created new energy packet "+energyPacket.Guid);
        }

        public void recalculatePaths(GUID connectionId)
        {
            //TODO: For now we just recalculate all paths! 
            foreach (var abstractNodeInstance in guidToNodesMapping.Values)
            {
                if (abstractNodeInstance is GeneratorInstance generatorInstance)
                {
                    foreach (var outputRouteStorage in EnergyRouter.createEnergyRoutes(generatorInstance))
                    {
                        outputRouteStorage.Key.RouteStorage = outputRouteStorage.Value;
                    }
                }
            }
        }

        public void tick(long tickCount, IFrontend frontend)
        {
            foreach (var timeSlice in timeSlices)
            {
                timeSlice.tick(tickCount);
            }
            
            _removeBuffer.Clear();

            foreach (var kvp in energyPackets) 
            {
                var packet = kvp.Value;
                packet.tick(tickCount, frontend);

                if (packet.Delivered)
                {
                    _removeBuffer.Add(kvp.Key);
                    onPacketDeleted?.Invoke(kvp.Key);
                    Debug.Log("Energy packet " + kvp.Key + " delivered.");
                }
            }

            // Remove after loop to avoid modifying collection during iteration. was causing errors
            for (int i = 0; i < _removeBuffer.Count; i++)
                energyPackets.Remove(_removeBuffer[i]);
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
                    Debug.Log("Generator "+generator.guid+" already connected to node "+(anyNode as AbstractNodeInstance).guid);
                    return null;
                }
                var foundOutput = generator.findFreeOutput();
                if (foundOutput == null)
                {
                    Debug.Log("Generator "+generator.guid+" has no free output!");

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
        public readonly int SliceNumber;

        public TimeSlice(SimulationStorage simulationStorage, int sliceNumber)
        {
            _simulationStorage = simulationStorage;
            SliceNumber = sliceNumber;
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

        public GUID? spawnRipple(Vector2 pos, EnergyType energyType)
        {
            if (spatialHashGrid.HasNodeNear(pos, TOLERANCE)) return null;
            var newNode = new TimeRippleInstance(pos, energyType);
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