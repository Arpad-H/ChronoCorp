using System.Collections.Generic;
using NodeBase;
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

            foreach (var output in generator.totalOutputs) result[output] = createEnergyRoute(output);

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

            var startRipple = potentialConnection.node1 is TimeRippleInstance
                ? potentialConnection.node1
                : potentialConnection.node2;

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
                for (var i = tempSteps.Count - 1; i >= 0; i--) route.addStep(tempSteps[i]);

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