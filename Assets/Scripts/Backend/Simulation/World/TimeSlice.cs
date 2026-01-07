using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Inv;
using Backend.Simulation.Energy;
using Interfaces;
using NodeBase;
using UnityEditor;
using UnityEngine;

namespace Backend.Simulation.World
{
    public class SimulationStorage
    {
        public const int TICKS_PER_SECOND = 50;
        private readonly List<GUID> _removeBuffer = new(128); //buffer to avoid modifying collection during iteration
        public readonly IFrontend Frontend;

        public readonly uint simulationSeed = 2930473240;
        public readonly StabilityBar StabilityBar = new(1000, 0, 500);
        public readonly List<TimeSlice> timeSlices = new();
        public Dictionary<GUID, EnergyPacket> energyPackets = new();
        public Dictionary<EnergyType, int> energyTypesAvailableInSimulation = new();
        public Dictionary<GUID, Connection> guidToConnections = new();
        public Dictionary<GUID, AbstractNodeInstance> guidToNodesMapping = new();
        public Inventory inventory = new();
        public Dictionary<NodeType, List<AbstractNodeInstance>> nodeTypeToNodesMapping = new();
        private int timeSliceNumCounter = 0;

        public SimulationStorage(IFrontend frontend)
        {
            Frontend = frontend;
            //timeSlices[0] = new TimeSlice(this);
            timeSlices.Add(new TimeSlice(this, timeSliceNumCounter++, 0)); //prevents out of bounds since starts of with count 0
        }

        public uint getTickSeed(long tickCount)
        {
            return (uint)(simulationSeed % tickCount + tickCount);
        }

        public int getAmountOutputsPlaced()
        {
            if (!nodeTypeToNodesMapping.TryGetValue(NodeType.GENERATOR, out var value))
            {
                return 0;
            }
            return value?.Sum(instance => ((GeneratorInstance)instance).AvailableOutputs.Count) ?? 0;
        }

        public int getAmountDifferentEnergyTypesInSimulation()
        {
            return energyTypesAvailableInSimulation.Count;
        }

        public HashSet<EnergyType> getEnergyTypesInSimulation()
        {
            return energyTypesAvailableInSimulation.Keys.ToHashSet();
        }

        public int getAmountOutputsTotal()
        {
            return getAmountOutputsPlaced() + inventory.nodesAvailable[NodeDTO.GENERATOR];
        }

        public event Action<GUID> onPacketDeleted;

        public bool isNodeKnown(GUID guid)
        {
            return guidToNodesMapping.ContainsKey(guid);
        }

        public void registerEnergyPacket(EnergyPacket energyPacket)
        {
            energyPackets[energyPacket.Guid] = energyPacket;
            Frontend.SpawnEnergyPacket(energyPacket.Guid, energyPacket.EnergyType); //inform frontend of new

            Debug.Log("Created new energy packet " + energyPacket.Guid);
        }

        public void recalculatePaths(GUID connectionId)
        {
            //TODO: For now we just recalculate all paths! 
            foreach (var abstractNodeInstance in guidToNodesMapping.Values)
                if (abstractNodeInstance is GeneratorInstance generatorInstance)
                    foreach (var outputRouteStorage in EnergyRouter.createEnergyRoutes(generatorInstance))
                        outputRouteStorage.Key.RouteStorage = outputRouteStorage.Value;
        }

        public void tick(long tickCount, IFrontend frontend)
        {
            foreach (var timeSlice in timeSlices) timeSlice.Tick(tickCount, this);

            _removeBuffer.Clear();

            foreach (var kvp in energyPackets)
            {
                var packet = kvp.Value;
                packet.Tick(tickCount, this);

                if (packet.Delivered)
                {
                    _removeBuffer.Add(kvp.Key);
                    onPacketDeleted?.Invoke(kvp.Key);
                    Debug.Log("Energy packet " + kvp.Key + " delivered.");
                }
            }

            StabilityBar.Tick(tickCount, this);

            // Remove after loop to avoid modifying collection during iteration. was causing errors
            for (var i = 0; i < _removeBuffer.Count; i++)
                energyPackets.Remove(_removeBuffer[i]);

            if (tickCount == 2000)
            {
                //TODO slice number was hardcoded. added counter as quick solution
                timeSlices.Add(new TimeSlice(this, timeSliceNumCounter++, 2000));
                Frontend.AddTimeSlice(timeSliceNumCounter-1); //pre increment otherwise it would be desynced
            }
        }

        public GUID? link(GUID idNode1, GUID idNode2)
        {
            var canPlace = inventory.canPlaceNormalConnection();
            if (!canPlace)
            {
                Debug.Log("Inventory prevents placing normal connection");
                return null;
            }
            if (!isNodeKnown(idNode1) || !isNodeKnown(idNode2) || !canPlace)
            {
                Debug.Log("Nodes not known to slice! " + idNode1 + " or " + idNode2);
                return null;
            }

            var node1 = guidToNodesMapping[idNode1];
            var node2 = guidToNodesMapping[idNode2];

            if (node1 is GeneratorInstance generator1 && node2 is GeneratorInstance generator2) return null;
            if (node1 is NodeWithConnections ripple1 && node2 is NodeWithConnections ripple2)
            {
                var eType1 = ((TimeRippleInstance)ripple1).EnergyType;
                var eType2 = ((TimeRippleInstance)ripple2).EnergyType;

                if (!eType1.Equals(EnergyType.WHITE) && !eType2.Equals(EnergyType.WHITE) &&
                    !eType1.Equals(eType2)) return null;

                if (ripple1.HasDirectConnectionTo(ripple2 as AbstractNodeInstance)) return null;

                inventory.placeNormalConnection();
                var rippleConnection = new Connection(ripple1 as AbstractNodeInstance, ripple2 as AbstractNodeInstance);
                ripple1.getConnections().Add(rippleConnection);
                ripple2.getConnections().Add(rippleConnection);
                guidToConnections[rippleConnection.guid] = rippleConnection;
                Debug.Log("Linked node " + node1.guid + " and " + node2.guid);
                return rippleConnection.guid;
            }

            if (node1 is GeneratorInstance gen1 && node2 is NodeWithConnections anyNode2)
                return linkGeneratorAndNonGenerator(gen1, anyNode2);

            if (node2 is GeneratorInstance gen2 && node1 is NodeWithConnections anyNode1)
                return linkGeneratorAndNonGenerator(gen2, anyNode1);

            GUID? linkGeneratorAndNonGenerator(GeneratorInstance generator, NodeWithConnections anyNode)
            {
                if (generator.alreadyConnectedTo(anyNode as AbstractNodeInstance))
                {
                    Debug.Log("Generator " + generator.guid + " already connected to node " +
                              (anyNode as AbstractNodeInstance).guid);
                    return null;
                }

                var foundOutput = generator.findFreeOutput();
                if (foundOutput == null)
                {
                    Debug.Log("Generator " + generator.guid + " has no free output!");

                    return null;
                }

                inventory.placeNormalConnection();
                var rippleConnection = new Connection(node1, node2);
                foundOutput.Connection = rippleConnection;
                anyNode.getConnections().Add(rippleConnection);
                guidToConnections[rippleConnection.guid] = rippleConnection;
                Debug.Log("Linked generator " + generator.guid + " to node " + (anyNode as AbstractNodeInstance).guid);
                return rippleConnection.guid;
            }

            return null;
        }

        public bool unlink(GUID connectionId)
        {
            var foundConnection = guidToConnections[connectionId];
            if (foundConnection == null) return false;

            inventory.removeNormalConnection();
            var node1 = foundConnection.node1;
            var node2 = foundConnection.node2;
            unlinkFromConnection(node1, foundConnection);
            unlinkFromConnection(node2, foundConnection);
            Debug.Log("Unlinked " + node1.guid + " and " + node2.guid);
            return true;

            void unlinkFromConnection(AbstractNodeInstance anyNode, Connection connection)
            {
                if (anyNode is GeneratorInstance generator)
                {
                    var foundOutput = generator.findOutputWithConnection(connection);
                    if (foundOutput != null) foundOutput.Connection = null;
                }

                if (anyNode is NodeWithConnections anyNodeWithConnections)
                    anyNodeWithConnections.getConnections().Remove(connection);
            }
        }
    }

    public class TimeSlice : ITickable
    {
        private const int TOLERANCE = 0;
        private readonly SimulationStorage _simulationStorage;
        public readonly int SliceNumber;
        private readonly long _tickPastDiff;
        public readonly NodeSpawner NodeSpawner;
        public readonly TimeSliceGrid TimeSliceGrid = new(16,9,1);

        public TimeSlice(SimulationStorage simulationStorage, int sliceNumber, long tickPastDiff)
        {
            _simulationStorage = simulationStorage;
            SliceNumber = sliceNumber;
            _tickPastDiff = tickPastDiff;
            NodeSpawner = new(this);
        }

        public void Tick(long tickCount, SimulationStorage storage)
        {
            tickCount -= _tickPastDiff;
            if (tickCount < 0)
            {
                tickCount = 0;
            }
            
            foreach (var abstractNodeInstance in _simulationStorage.guidToNodesMapping.Values)
                abstractNodeInstance.Tick(tickCount, _simulationStorage);
            NodeSpawner.Tick(tickCount, _simulationStorage);
        }

        public GUID? spawnGenerator(Vector2 pos, int amountInitialOutputs)
        {
            var canPlace = _simulationStorage.inventory.canPlaceGenerator();

            if (TimeSliceGrid.HasNodeNear(pos, TOLERANCE) || !canPlace) return null;

            _simulationStorage.inventory.placeGenerator();
            var newNode = new GeneratorInstance(pos, amountInitialOutputs);
            TimeSliceGrid.Add(newNode);

            addNodeToMapping(newNode);
            return newNode.guid;
        }

        public GUID? spawnRipple(Vector2 pos, EnergyType energyType, out TimeRippleInstance timeRippleInstance)
        {
            timeRippleInstance = null;
            if (TimeSliceGrid.HasNodeNear(pos, TOLERANCE)) return null;
            var newNode = new TimeRippleInstance(pos, energyType);
            TimeSliceGrid.Add(newNode);
            addNodeToMapping(newNode);
            timeRippleInstance = newNode;

            if (!_simulationStorage.energyTypesAvailableInSimulation.ContainsKey(energyType))
            {
                _simulationStorage.energyTypesAvailableInSimulation[energyType] = 0;
            }
            
            _simulationStorage.energyTypesAvailableInSimulation[energyType]++;
            return newNode.guid;
        }

        public bool removeNode(GUID guid)
        {
            if (_simulationStorage.guidToNodesMapping.TryGetValue(guid, out var nodeInstance))
                if (TimeSliceGrid.Remove(nodeInstance))
                {
                    if (nodeInstance is GeneratorInstance) _simulationStorage.inventory.removeGenerator();
                    if (nodeInstance is TimeRippleInstance timeRippleInstance)
                    {
                        if (_simulationStorage.energyTypesAvailableInSimulation.ContainsKey(timeRippleInstance.EnergyType))
                        {
                            _simulationStorage.energyTypesAvailableInSimulation[timeRippleInstance.EnergyType]--;
                            if (_simulationStorage.energyTypesAvailableInSimulation[timeRippleInstance.EnergyType] <= 0)
                            {
                                _simulationStorage.energyTypesAvailableInSimulation.Remove(timeRippleInstance.EnergyType);
                            }
                        }
                    }
                    removeNodeFromMapping(nodeInstance);

                    return true;
                }

            return false;
        }

        public bool isNodeKnown(GUID guid)
        {
            return _simulationStorage.isNodeKnown(guid);
        }

        private void addNodeToMapping(AbstractNodeInstance newNode)
        {
            _simulationStorage.guidToNodesMapping.Add(newNode.guid, newNode);

            if (!_simulationStorage.nodeTypeToNodesMapping.ContainsKey(newNode.NodeType))
                _simulationStorage.nodeTypeToNodesMapping.Add(newNode.NodeType, new List<AbstractNodeInstance>());
            _simulationStorage.nodeTypeToNodesMapping[newNode.NodeType].Add(newNode);
            Debug.Log("Created node " + newNode.guid);
        }

        private void removeNodeFromMapping(AbstractNodeInstance newNode)
        {
            _simulationStorage.guidToNodesMapping.Remove(newNode.guid);
            if (_simulationStorage.nodeTypeToNodesMapping.ContainsKey(newNode.NodeType))
                _simulationStorage.nodeTypeToNodesMapping.Remove(newNode.NodeType);
            Debug.Log("Removed node " + newNode.guid);
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