using System.Collections.Generic;
using Backend.Simulation.World;
using Interfaces;
using NodeBase;
using UnityEditor;
using UnityEngine;

namespace Backend.Simulation.Energy
{
    public class EnergyRouter
    {
        /**
         * Creates route storages for all outputs on a generator
         */
        public static Dictionary<Output, OutputRouteStorage> createEnergyRoutes(GeneratorInstance generator)
        {
            var result = new Dictionary<Output, OutputRouteStorage>();

            Debug.Log("Creating energy routes for " + generator.guid);

            foreach (var output in generator.AvailableOutputs)
            {
                result[output] = createEnergyRoute(output);
            }

            return result;
        }

        /**
         * Creates an output storage for one output based on bfs.
         */
        public static OutputRouteStorage createEnergyRoute(Output output)
        {
            var routesForOutput = new OutputRouteStorage();

            var potentialConnection = output.Connection;
            if (potentialConnection == null) return routesForOutput;

            var startRipple = potentialConnection.node2;

            var alreadyVisitedNodes = new Dictionary<AbstractNodeInstance, NodeWithConnections>();
            var alreadyVisitedConnections = new Dictionary<AbstractNodeInstance, Connection>();

            BfsFromRipple((TimeRippleInstance)startRipple, alreadyVisitedConnections, alreadyVisitedNodes);

            var generatorToRippleConnection = output.Connection;
            if (generatorToRippleConnection == null) return routesForOutput;

            // Erste Route: Generator -> startRipple
            var firstRoute = new EnergyRoute();

            // Richtung der ersten Kante korrekt bestimmen:
            // Wenn node1 == startRipple, dann kommt der Generator von node2 -> node1,
            // also muss reverseDirection = true sein, damit Start = node2 (Generatorseite) ist.
            var firstStepReverse =
                generatorToRippleConnection.node1 == startRipple;
            var firstStep = new EnergyStep(
                generatorToRippleConnection,
                firstStepReverse
            );

            firstRoute.addStep(firstStep);
            routesForOutput.addRoute(startRipple, firstRoute);

            // Für alle anderen Knoten den Pfad Generator -> startRipple -> ... -> Node aufbauen
            foreach (var kv in alreadyVisitedConnections)
            {
                var nodeTargetOfConnection = kv.Key;
                if (ReferenceEquals(nodeTargetOfConnection, startRipple)) continue;

                // Temporäre Liste der Schritte von startRipple bis zum Zielknoten
                // (wird zunächst rückwärts aufgebaut)
                var tempSteps = new List<EnergyStep>();

                var currentNodeInTraversal = nodeTargetOfConnection;
                while (currentNodeInTraversal != null && !ReferenceEquals(currentNodeInTraversal, startRipple))
                {
                    if (!alreadyVisitedConnections.TryGetValue(currentNodeInTraversal, out var edge))
                        break;

                    var from = alreadyVisitedNodes[currentNodeInTraversal];
                    var directionReversed = edge.node1 != from;

                    var step = new EnergyStep(edge, directionReversed);
                    tempSteps.Add(step);

                    currentNodeInTraversal = from as AbstractNodeInstance;
                }

                // Wenn wir nicht sauber beim Start-Ripple angekommen sind, Pfad verwerfen
                if (!ReferenceEquals(currentNodeInTraversal, startRipple))
                    continue;

                // Route endgültig in richtiger Reihenfolge aufbauen:
                // 1. Generator -> startRipple
                // 2. startRipple -> ... -> nodeTargetOfConnection
                var route = new EnergyRoute();

                // Schritt vom Generator zum Start-Ripple immer zuerst
                route.addStep(firstStep);

                // tempSteps sind rückwärts (von Target zurück zu startRipple) aufgebaut,
                // also umgedreht anhängen, damit wir von startRipple nach außen laufen.
                for (int i = tempSteps.Count - 1; i >= 0; i--)
                {
                    route.addStep(tempSteps[i]);
                }

                routesForOutput.addRoute(nodeTargetOfConnection, route);
            }

            return routesForOutput;

            void BfsFromRipple(
                TimeRippleInstance start,
                IDictionary<AbstractNodeInstance, Connection> alreadyVisitedConnectionsInner,
                IDictionary<AbstractNodeInstance, NodeWithConnections> alreadyVisitedNodesInner)
            {
                var queue = new Queue<NodeWithConnections>();
                queue.Enqueue(start);
                alreadyVisitedNodesInner[start] = null;

                while (queue.Count > 0)
                {
                    var nextRipple = queue.Dequeue();

                    foreach (var nextConnection in nextRipple.getConnections())
                    {
                        var potentialNextStop = nextConnection.node2 == nextRipple
                            ? nextConnection.node1
                            : nextConnection.node2;
                        if (potentialNextStop is not NodeWithConnections nextStopNode) continue;

                        if (!alreadyVisitedNodesInner.ContainsKey(potentialNextStop))
                        {
                            // WICHTIG: Vorgänger speichern, nicht den Knoten selbst
                            alreadyVisitedNodesInner[potentialNextStop] = nextRipple;
                            alreadyVisitedConnectionsInner[potentialNextStop] = nextConnection;
                            queue.Enqueue(nextStopNode);
                        }
                    }
                }
            }
        }
    }

    public class EnergyScheduler
    {
        private const int TickCooldownOutputs = 400;

        // For one output try to spawn a new packet via cooldown.
        public static void tick(long currentTick, Output output, SimulationStorage storage)
        {
            output.RouteStorage ??= EnergyRouter.createEnergyRoute(output);

            if (output.RouteStorage.savedRoutes == null || output.RouteStorage.savedRoutes.Count == 0) return;
            var last = output.lastGenerationTick;

            if (currentTick - last < TickCooldownOutputs) return;

            var nextIndex = output.targetIndex++ % output.RouteStorage.savedRoutes.Count;
            var nextRoute = output.RouteStorage.orderedListOfRoutes[nextIndex];

            var stepsInRoute = nextRoute.steps.Count;
            if (stepsInRoute == 0) return;
            var startStep = nextRoute.steps[0];
            var endStep = nextRoute.steps[stepsInRoute - 1];

            var newEnergyPacket = new EnergyPacket(
                ChooseEnergyType(endStep.getEnd()),
                startStep.getStart() as EnergyPacketSpawner,
                endStep.getEnd(),
                nextRoute.steps
            );

            storage.registerEnergyPacket(newEnergyPacket);
            output.lastGenerationTick = currentTick;
        }

        /**
         * Used to determine which energy type to select for a new energy packet based on the energy type a node accepts.
         */
        private static EnergyType ChooseEnergyType(AbstractNodeInstance anyNode)
        {
            if (anyNode is TimeRippleInstance ripple) return ripple.EnergyType;
            return EnergyType.WHITE;
        }
    }

    public class EnergyPacket
    {
        private const float PacketTravelSpeedPerTick = 0.1f;

        private int _currentEdgeIndex;

        public bool Delivered;
        private float _travelledOnEdge;
        public GUID Guid { get; }

        public EnergyPacket(
            EnergyType energyType,
            EnergyPacketSpawner source,
            AbstractNodeInstance destination,
            List<EnergyStep> steps)
        {
            Guid = GUID.Generate();
            EnergyType = energyType;
            Source = source;
            Destination = destination;
            Steps = steps;
        }

        // Units traveled along the edge
        public float progressOnEdge { get; set; }

        public EnergyType EnergyType { get; }

        public EnergyPacketSpawner Source { get; set; }
        public AbstractNodeInstance Destination { get; set; }
        private List<EnergyStep> Steps { get; }

        public void tick(long tick, IFrontend frontend)
        {
            if (Delivered) return;
            _travelledOnEdge += PacketTravelSpeedPerTick;
            progressOnEdge = _travelledOnEdge / currentStep().connection.length;
            if (progressOnEdge < 1.0f) return;

            progressOnEdge = 0;
            _travelledOnEdge = 0;
            _currentEdgeIndex++;
            if (_currentEdgeIndex >= Steps.Count)
            {
                Debug.Log("Delivered at target");
                Delivered = true;
            }
            else
            {
                Debug.Log("Relaying to next one");
            }
        }

        public EnergyStep currentStep()
        {
            return Steps[_currentEdgeIndex];
        }
    }

    public class OutputRouteStorage
    {
        public Dictionary<AbstractNodeInstance, EnergyRoute> savedRoutes { get; } = new();
        public List<EnergyRoute> orderedListOfRoutes { get; } = new();

        public void addRoute(AbstractNodeInstance node, EnergyRoute route)
        {
            savedRoutes[node] = route;
            orderedListOfRoutes.Add(route);
        }

        public EnergyRoute getRoute(AbstractNodeInstance node)
        {
            return savedRoutes[node];
        }
    }

    public class EnergyRoute
    {
        public List<EnergyStep> steps { get; } = new();

        public void addStep(EnergyStep step)
        {
            steps.Add(step);
        }
    }

    public class EnergyStep
    {
        public EnergyStep(Connection connection, bool reverseDirection)
        {
            this.connection = connection;
            this.reverseDirection = reverseDirection;
        }

        public Connection connection { get; }

        // true -> 1 to 2 else 2 to 1
        public bool reverseDirection { get; }

        public AbstractNodeInstance getStart()
        {
            return reverseDirection ? connection.node2 : connection.node1;
        }

        public AbstractNodeInstance getEnd()
        {
            return reverseDirection ? connection.node1 : connection.node2;
        }
    }
}
