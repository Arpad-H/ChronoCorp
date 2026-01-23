using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Inv;
using Backend.Simulation.Energy;
using Backend.Simulation.SimEvent;
using Interfaces;
using JetBrains.Annotations;
using NodeBase;
using UnityEngine;
using Util;

namespace Backend.Simulation.World
{
    public class SimulationStorage
    {
        public const int TICKS_PER_SECOND = 50;
        private readonly List<Guid> _removeBuffer = new(128); //buffer to avoid modifying collection during iteration
        public readonly IFrontend Frontend;

        public readonly uint simulationSeed = 2930473240;
        public readonly StabilityBar StabilityBar = new(BalanceProvider.Balance.stabilityMaxValue, BalanceProvider.Balance.stabilityMinValue, BalanceProvider.Balance.stabilityMaxValue);
        public readonly List<TimeSlice> timeSlices = new();
        public Dictionary<Guid, EnergyPacket> energyPackets = new();
        public Dictionary<Guid, Connection> guidToConnections = new();
        public Dictionary<Guid, AbstractNodeInstance> guidToNodesMapping = new();
        public Inventory inventory = new();
        public Dictionary<NodeType, List<AbstractNodeInstance>> nodeTypeToNodesMapping = new();
        private int timeSliceNumCounter = 0;
        public SimEventLog log = new();
        
        private DifficultyTargets targets = new DifficultyTargets
        {
            roundLengthSeconds = 12 * 60,
            easyPhaseSeconds = 1 * 60,
            ripplesAtEasyEnd = 6,
            ripplesAtRoundEnd = 40,
            stabilityFractionAtEasyEnd = 0.90f,
            stabilityFractionAtRoundEnd = 0.15f,
            ripplesSustainablePerOutput = 1.5f,
            minSpawnIntervalSeconds = 7.0f,
            maxSpawnIntervalSeconds = 15.0f
        };

        public SimulationStorage(IFrontend frontend)
        {
            Frontend = frontend;
            //timeSlices[0] = new TimeSlice(this);
            timeSlices.Add(new TimeSlice(this, timeSliceNumCounter++, 0));//prevents out of bounds since starts of with count 0
            Frontend.AddTimeSlice(timeSliceNumCounter-1); //pre increment otherwise it would be desynced
            if (BalanceProvider.Balance.useAutoBalance)BalanceAutoTuner.ApplyAutoTune(BalanceProvider.Balance, targets);
        }

        public uint getTickSeed(long tickCount)
        {
            tickCount = tickCount == 0 ? 1 : tickCount;
            return (uint)(simulationSeed % tickCount + tickCount);
        }

        public bool deleteNode(Guid nodeBackendId)
        {
            var timeSlice = getTimeSliceOfNodeByGuid(nodeBackendId);
            if (timeSlice == null)
            {
                return false;
            }
            guidToNodesMapping.TryGetValue(nodeBackendId, out var foundNode);
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
                    recalculatePaths();
                    return timeSlice?.removeNode(nodeBackendId) ?? false;
                }
                case NodeWithConnections node:
                {
                    var connectionsToRemove = new List<Connection>(node.getConnections().Count);
                    foreach (var connection in node.getConnections())
                    {
                        connectionsToRemove.Add(connection);
                    }
                    foreach (var connection in connectionsToRemove)
                    {
                        UnlinkNodes(connection.guid, false);
                    }
                    recalculatePaths();
                    return timeSlice?.removeNode(nodeBackendId) ?? false;
                }
                default:
                    return false;
            }
        }
        
        public bool UnlinkNodes(Guid connectionId, bool recalculatePaths)
        {
            foreach (var slice in timeSlices)
            {
                slice.TimeSliceGrid.RemoveConnectionCells(connectionId);
            }

            if (unlink(connectionId))
            {
                if(recalculatePaths) this.recalculatePaths();
                return true;
            }

            return false;
        }

        public int getAmountOutputsPlaced()
        {
            if (!nodeTypeToNodesMapping.TryGetValue(NodeType.GENERATOR, out var value))
            {
                return 0;
            }
            return value?.Sum(instance => ((GeneratorInstance)instance).totalOutputs.Count) ?? 0;
        }

        public int getAmountOutputsTotal()
        {
            return getAmountOutputsPlaced() + inventory.inventoryItemsAvailable[InventoryItem.GENERATOR];
        }

        public event Action<Guid> onPacketDeleted;

        public bool isNodeKnown(Guid guid)
        {
            return guidToNodesMapping.ContainsKey(guid);
        }

        public void registerEnergyPacket(EnergyPacket energyPacket)
        {
            energyPackets[energyPacket.Guid] = energyPacket;
            Frontend.SpawnEnergyPacket(energyPacket.Guid, energyPacket.EnergyType); //inform frontend of new
        }

        public void recalculatePaths()
        {
            foreach (var abstractNodeInstance in guidToNodesMapping.Values)
                if (abstractNodeInstance is GeneratorInstance generatorInstance)
                    foreach (var outputRouteStorage in EnergyRouter.createEnergyRoutes(generatorInstance))
                        outputRouteStorage.Key.RouteStorage = outputRouteStorage.Value;
        }

        public void tick(long tickCount, IFrontend frontend)
        {
            //BalanceAutoTuner.Tick(tickCount, this);
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
                    //Debug.Log("Energy packet " + kvp.Key + " delivered.");
                }
            }

            StabilityBar.Tick(tickCount, this);

            // Remove after loop to avoid modifying collection during iteration. was causing errors
            for (var i = 0; i < _removeBuffer.Count; i++)
                energyPackets.Remove(_removeBuffer[i]);

            if (tickCount % BalanceProvider.Balance.layerDuplicationTime == 0 && tickCount != 0 && timeSlices.Count < BalanceProvider.Balance.maxLayerCount)
            {
                int newTimeSliceNumber = timeSliceNumCounter++;
                timeSlices.Add(new TimeSlice(this, newTimeSliceNumber, newTimeSliceNumber * BalanceProvider.Balance.layerDuplicationTime));
                Frontend.AddTimeSlice(timeSliceNumCounter-1); //pre increment otherwise it would be desynced
            }
        }

        public Guid? link(Guid idNode1, Guid idNode2, Vector2Int[] cellsOfConnection)
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
                var eType1 = ripple1.getAcceptedEnergyType();
                var eType2 = ripple2.getAcceptedEnergyType();

                if (!eType1.Equals(EnergyType.WHITE) && !eType2.Equals(EnergyType.WHITE) &&
                    !eType1.Equals(eType2)) return null;

                if (ripple1.HasDirectConnectionTo(ripple2 as AbstractNodeInstance)) return null;

                inventory.placeNormalConnection();
                var rippleConnection = new Connection(ripple1 as AbstractNodeInstance, ripple2 as AbstractNodeInstance, cellsOfConnection);
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

            Guid? linkGeneratorAndNonGenerator(GeneratorInstance generator, NodeWithConnections anyNode)
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
                var rippleConnection = new Connection(node1, node2, cellsOfConnection);
                foundOutput.Connection = rippleConnection;
                anyNode.getConnections().Add(rippleConnection);
                guidToConnections[rippleConnection.guid] = rippleConnection;
                Debug.Log("Linked generator " + generator.guid + " to node " + (anyNode as AbstractNodeInstance).guid);
                return rippleConnection.guid;
            }

            return null;
        }

        public bool unlink(Guid connectionId)
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
        
        [CanBeNull]
        public TimeSlice getTimeSliceOfNodeByGuid(Guid guid)
        {
            guidToNodesMapping.TryGetValue(guid, out var node);
            if (node != null)
            {
                return node.currentTimeSlice;
            }

            return null;
        }
    }

    public class TimeSlice : ITickable
    {
        private const int TOLERANCE = 0;
        public readonly SimulationStorage _simulationStorage;
        public readonly int SliceNumber;
        private readonly long _tickPastDiff;
        public readonly NodeSpawner NodeSpawner;
        public readonly TimeSliceGrid TimeSliceGrid = new(BalanceProvider.Balance.layerGridCellCount.x, BalanceProvider.Balance.layerGridCellCount.y, BalanceProvider.Balance.cellSize);
        public Dictionary<EnergyType, int> energyTypesAvailableInSimulation = new();
        private TimeSliceRunner _runner;

        public TimeSlice(SimulationStorage simulationStorage, int sliceNumber, long tickPastDiff)
        {
            _simulationStorage = simulationStorage;
            SliceNumber = sliceNumber;
            _tickPastDiff = tickPastDiff;
            NodeSpawner = new(this);
            _runner = new (this, _simulationStorage.log,tickPastDiff);
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
            if (SliceNumber == 0)
            {
                NodeSpawner.Tick(tickCount, _simulationStorage);
            }
            else
            {
                _runner.Tick(tickCount, storage);
            }
        }
        
        public int getAmountDifferentEnergyTypesInSimulation()
        {
            return energyTypesAvailableInSimulation.Count;
        }

        public HashSet<EnergyType> getEnergyTypesInSimulation()
        {
            return energyTypesAvailableInSimulation.Keys.ToHashSet();
        }

        public Guid? spawnGenerator(Vector2 pos, int amountInitialOutputs)
        {
            var canPlace = _simulationStorage.inventory.canPlaceGenerator();

            
            if (TimeSliceGrid.IsCellOccupied(new Vector2Int((int)pos.x, (int)pos.y)) || !canPlace) return null;

            _simulationStorage.inventory.placeGenerator();
            var newNode = new GeneratorInstance(pos, amountInitialOutputs);
            newNode.currentTimeSlice = this;
            TimeSliceGrid.Add(newNode);

            addNodeToMapping(newNode);
            return newNode.guid;
        }

        public Guid? spawnBlackHole(Vector2 pos, out BlackHoleInstance blackHoleInstance)
        {
            blackHoleInstance = null;
            if (TimeSliceGrid.IsCellOccupied(new Vector2Int((int)pos.x, (int)pos.y), out var node, out var connection))
            {
                if (node != null)
                {
                    if (_simulationStorage.deleteNode(node.guid))
                    {
                        _simulationStorage.Frontend.DeleteNode(node.guid);
                    }
                    else
                    {
                        Debug.Log("Backend could not delete node to replace it with a black hole!");
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            blackHoleInstance = new BlackHoleInstance(pos);
            blackHoleInstance.currentTimeSlice = this;
            TimeSliceGrid.Add(blackHoleInstance);
            addNodeToMapping(blackHoleInstance);
            return blackHoleInstance.guid;
        }
        
        public Guid? spawnBlockadeInConnection(Connection connection, Vector2 cellPos, out BlockadeNodeInstance blockadeNodeInstance )
        {
            blockadeNodeInstance = null;

            var nodeStart = connection.node1;
            var nodeFinish = connection.node2;
            
            var cellsOfConnection = TimeSliceGrid.getCellsOfConnection(connection.guid);
            var newConnectionStartToBlockade = new List<Vector2Int>();
            var newConnectionBlockadeToFinish = new List<Vector2Int>();

            var foundMiddlePiece = false;
            foreach (var cell in cellsOfConnection)
            {
                if (cell.Equals(new Vector2Int((int)cellPos.x, (int)cellPos.y)))
                {
                    foundMiddlePiece = true;
                    continue;
                }
                
                if (!foundMiddlePiece)
                {
                    newConnectionStartToBlockade.Add(cell);
                }
                else
                {
                    newConnectionBlockadeToFinish.Add(cell);
                }
            }
            if (newConnectionBlockadeToFinish.Count == 0 || newConnectionStartToBlockade.Count == 0)
            {
                return null;
            }

            if (!_simulationStorage.unlink(connection.guid))
            {
                return null;
            }
            _simulationStorage.Frontend.DeleteConnection(connection.guid);

            BlockadeNodeInstance newNode = new BlockadeNodeInstance(cellPos);
            newNode.currentTimeSlice = this;
            TimeSliceGrid.Add(newNode);
            addNodeToMapping(newNode);
            var startConID = _simulationStorage.link(nodeStart.guid, newNode.guid, newConnectionStartToBlockade.ToArray());
            var endConID = _simulationStorage.link(newNode.guid, nodeFinish.guid, newConnectionBlockadeToFinish.ToArray());

            if (startConID == null || endConID == null)
            {
                throw new Exception("Could n't link node to start or end node!");
            }

            _simulationStorage.Frontend.PlaceNodeVisual(newNode.guid, newNode.NodeType.NodeDTO, this.SliceNumber, new Vector2(newNode.Pos.x, newNode.Pos.y), EnergyType.WHITE);
            _simulationStorage.Frontend.CreateConnection(nodeStart.guid, newNode.guid, (Guid)startConID, newConnectionStartToBlockade.ToArray());
            _simulationStorage.Frontend.CreateConnection(newNode.guid, nodeFinish.guid, (Guid)endConID, newConnectionBlockadeToFinish.ToArray());
            
            blockadeNodeInstance = newNode;
            return blockadeNodeInstance.guid;
        }

        public Guid? spawnRipple(Vector2 pos, EnergyType energyType, out TimeRippleInstance timeRippleInstance)
        {
            timeRippleInstance = null;
            if(TimeSliceGrid.IsCellOccupied(new Vector2Int((int)pos.x, (int)pos.y), out var node, out var connection)) return null;
            var newNode = new TimeRippleInstance(pos, energyType);
            newNode.currentTimeSlice = this;
            TimeSliceGrid.Add(newNode);
            addNodeToMapping(newNode);
            timeRippleInstance = newNode;

            if (!energyTypesAvailableInSimulation.ContainsKey(energyType))
            {
                energyTypesAvailableInSimulation[energyType] = 0;
            }
            
            energyTypesAvailableInSimulation[energyType]++;
            return newNode.guid;
        }

        public bool removeNode(Guid guid)
        {
            Debug.Log("Removing " + guid);
            if (_simulationStorage.guidToNodesMapping.TryGetValue(guid, out var nodeInstance))
                if (TimeSliceGrid.Remove(nodeInstance))
                {
                    if (nodeInstance is GeneratorInstance) _simulationStorage.inventory.removeGenerator();
                    if (nodeInstance is TimeRippleInstance timeRippleInstance)
                    {
                        if (energyTypesAvailableInSimulation.ContainsKey(timeRippleInstance.EnergyType))
                        {
                            energyTypesAvailableInSimulation[timeRippleInstance.EnergyType]--;
                            if (energyTypesAvailableInSimulation[timeRippleInstance.EnergyType] <= 0)
                            {
                                energyTypesAvailableInSimulation.Remove(timeRippleInstance.EnergyType);
                            }
                        }
                    }
                    removeNodeFromMapping(nodeInstance);

                    return true;
                }

            return false;
        }

        public bool isNodeKnown(Guid guid)
        {
            return _simulationStorage.guidToNodesMapping.TryGetValue(guid, out var nodeInstance) && nodeInstance.currentTimeSlice.SliceNumber == SliceNumber;
        }

        private void addNodeToMapping(AbstractNodeInstance newNode)
        {
            _simulationStorage.guidToNodesMapping.Add(newNode.guid, newNode);

            if (!_simulationStorage.nodeTypeToNodesMapping.ContainsKey(newNode.NodeType))
                _simulationStorage.nodeTypeToNodesMapping.Add(newNode.NodeType, new List<AbstractNodeInstance>());
            _simulationStorage.nodeTypeToNodesMapping[newNode.NodeType].Add(newNode);
            Debug.Log("Placed node " + newNode.guid+" in slice "+SliceNumber);
        }

        private void removeNodeFromMapping(AbstractNodeInstance newNode)
        {
            _simulationStorage.guidToNodesMapping.Remove(newNode.guid);
            if (_simulationStorage.nodeTypeToNodesMapping.ContainsKey(newNode.NodeType))
                _simulationStorage.nodeTypeToNodesMapping.Remove(newNode.NodeType);
            Debug.Log("Removed node " + newNode.guid+" from slice "+SliceNumber);
        }
    }
}