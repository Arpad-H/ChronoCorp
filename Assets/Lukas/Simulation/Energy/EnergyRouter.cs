using System.Collections.Generic;
using NodeBase;

namespace Lukas.Simulation.Energy
{
    public class EnergyRouter
    {
        /**
         * Creates route storages for all outputs on a generator
         */
        public static Dictionary<Output, OutputRouteStorage> createEnergyRoutes(GeneratorInstance generator)
        {
            var result = new Dictionary<Output, OutputRouteStorage>();

            foreach (var output in generator.AvailableOutputs) result[output] = createEnergyRoute(output);

            return result;
        }

        /**
         * Creates an output storage for one output based on bsf.
         */
        public static OutputRouteStorage createEnergyRoute(Output output)
        {
            var routesForOutput = new OutputRouteStorage();

            var potentialConnection = output.Connection;
            if (potentialConnection == null) return routesForOutput;

            var startRipple = potentialConnection.node2;

            var alreadyVisitedNodes = new Dictionary<AbstractNodeInstance, NodeWithConnections>();
            var alreadyVisitedConnections =
                new Dictionary<AbstractNodeInstance, Connection<AbstractNodeInstance, AbstractNodeInstance>>();

            BfsFromRipple(startRipple, alreadyVisitedConnections, alreadyVisitedNodes);

            var generatorToRippleConnection = output.Connection;
            if (generatorToRippleConnection == null) return routesForOutput;

            var firstRoute = new EnergyRoute();
            var firstStep =
                new EnergyStep(
                    (Connection<AbstractNodeInstance, AbstractNodeInstance>)(object)generatorToRippleConnection,
                    false);
            firstRoute.addStep(firstStep);
            routesForOutput.addRoute(startRipple, firstRoute);

            foreach (var kv in alreadyVisitedConnections)
            {
                var nodeTargetOfConnection = kv.Key;
                if (ReferenceEquals(nodeTargetOfConnection, startRipple)) continue;


                var route = new EnergyRoute();
                var currentNodeInTraversal = nodeTargetOfConnection;
                while (nodeTargetOfConnection != null && !ReferenceEquals(currentNodeInTraversal, startRipple))
                {
                    if (!alreadyVisitedConnections.TryGetValue(currentNodeInTraversal, out var edge))
                        break;

                    var from = alreadyVisitedNodes[currentNodeInTraversal];
                    var directionReversed = edge.node1 != from;

                    var step = new EnergyStep(edge, directionReversed);
                    route.addStep(step);

                    currentNodeInTraversal = (AbstractNodeInstance)from;
                }

                routesForOutput.addRoute(nodeTargetOfConnection, route);
            }

            return routesForOutput;

            void BfsFromRipple(
                TimeRippleInstance start,
                IDictionary<AbstractNodeInstance, Connection<AbstractNodeInstance, AbstractNodeInstance>>
                    alreadyVisitedConnections,
                IDictionary<AbstractNodeInstance, NodeWithConnections> alreadyVisitedNodes)
            {
                var queue = new Queue<NodeWithConnections>();
                queue.Enqueue(start);
                alreadyVisitedNodes[start] = null;

                while (queue.Count > 0)
                {
                    var nextRipple = queue.Dequeue();

                    foreach (var nextConnection in nextRipple.getConnections())
                    {
                        var potentialNextStop = nextConnection.node2 == nextRipple
                            ? nextConnection.node1
                            : nextConnection.node2;
                        if (potentialNextStop is not NodeWithConnections nextStopNode) continue;

                        if (!alreadyVisitedNodes.ContainsKey(potentialNextStop))
                        {
                            alreadyVisitedNodes[potentialNextStop] = nextStopNode;
                            alreadyVisitedConnections[potentialNextStop] = nextConnection;
                            queue.Enqueue(nextStopNode);
                        }
                    }
                }
            }
        }
    }

    public class EnergyScheduler
    {
        private const int TickCooldownOutputs = 100;

        // For one output try to spawn a new packet via cooldown.
        public static void tick(int currentTick, Output output, List<EnergyPacket> packetsSpawnedThisTick)
        {
            output.RouteStorage ??= EnergyRouter.createEnergyRoute(output);

            if (output.RouteStorage.savedRoutes == null || output.RouteStorage.savedRoutes.Count == 0) return;
            var last = output.lastGenerationTick;

            if (currentTick - last < TickCooldownOutputs) return;

            var nextRoute =
                output.RouteStorage.orderedListOfRoutes[
                    output.targetIndex++ % output.RouteStorage.savedRoutes.Count];
            var stepsInRoute = nextRoute.steps.Count;
            var startStep = nextRoute.steps[0];
            var endStep = nextRoute.steps[stepsInRoute];

            var newEnergyPacket = new EnergyPacket(ChooseEnergyType(endStep.getEnd()), startStep.getStart() as EnergyPacketSpawner, endStep.getEnd(), nextRoute.steps);
            packetsSpawnedThisTick.Add(newEnergyPacket);
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
        private const int PacketTravelSpeedPerTick = 1;

        private int _currentEdgeIndex;

        private bool _delivered;

        // Units traveled along the edge
        private float _progressOnEdge;
        private int _travelledOnEdge;

        public EnergyPacket(EnergyType energyType, EnergyPacketSpawner source, AbstractNodeInstance destination,
            List<EnergyStep> steps)
        {
            EnergyType = energyType;
            Source = source;
            Destination = destination;
            Steps = steps;
        }

        public EnergyType EnergyType { get; }

        public EnergyPacketSpawner Source { get; set; }
        public AbstractNodeInstance Destination { get; set; }
        private List<EnergyStep> Steps { get; }


        public void tick()
        {
            if (_delivered) return;
            _travelledOnEdge += PacketTravelSpeedPerTick;
            _progressOnEdge = _travelledOnEdge / currentStep().connection.length;
            if (!(_progressOnEdge >= 1.0)) return;

            _progressOnEdge = 0;
            _travelledOnEdge = 0;
            _currentEdgeIndex++;
            if (_currentEdgeIndex >= Steps.Count) _delivered = true;
        }

        private EnergyStep currentStep()
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
        public EnergyStep(Connection<AbstractNodeInstance, AbstractNodeInstance> connection, bool reverseDirection)
        {
            this.connection = connection;
            this.reverseDirection = reverseDirection;
        }

        public Connection<AbstractNodeInstance, AbstractNodeInstance> connection { get; }

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