using System.Collections.Generic;
using NodeBase;
using NUnit.Framework;

namespace Lukas.Simulation.Energy
{
    public class EnergyRouter
    {
        public static Dictionary<Output, OutputRouteStorage> createEnergyRoutes(GeneratorInstance generator)
        {
            var result = new Dictionary<Output, OutputRouteStorage>();

            foreach (var output in generator.AvailableOutputs)
            {
                var routesForOutput = new OutputRouteStorage();

                var potentialConnection = output.Connection;
                if (potentialConnection == null)
                {
                    result[output] = routesForOutput;
                    continue;
                }

                var startRipple = potentialConnection.node2;

                var alreadyVisitedNodes = new Dictionary<AbstractNodeInstance, NodeWithConnections>();
                var alreadyVisitedConnections = new Dictionary<AbstractNodeInstance, Connection<AbstractNodeInstance, AbstractNodeInstance>>();

                BfsFromRipple(startRipple, alreadyVisitedConnections, alreadyVisitedNodes);
                
                var generatorToRippleConnection = output.Connection;
                if (generatorToRippleConnection == null)
                {
                    continue;
                }
                
                var firstRoute = new EnergyRoute();
                var firstStep = new EnergyStep((Connection<AbstractNodeInstance, AbstractNodeInstance>)(object)generatorToRippleConnection, false);
                firstRoute.addStep(firstStep);
                routesForOutput.addRoute(startRipple, firstRoute);
                
                foreach (var kv in alreadyVisitedConnections)
                {
                    var nodeTargetOfConnection = kv.Key;
                    if (ReferenceEquals(nodeTargetOfConnection, startRipple))
                    {
                        continue;
                    }

                    
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

                        currentNodeInTraversal = (AbstractNodeInstance) from;
                    }
                    routesForOutput.addRoute(nodeTargetOfConnection, route);
                }

                result[output] = routesForOutput;
            }

            return result;

            void BfsFromRipple(
                TimeRippleInstance start,
                IDictionary<AbstractNodeInstance, Connection<AbstractNodeInstance, AbstractNodeInstance>> alreadyVisitedConnections,
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
                        var potentialNextStop = nextConnection.node2 == nextRipple ? nextConnection.node1 : nextConnection.node2;
                        if (potentialNextStop is not NodeWithConnections nextStopNode)
                        {
                            continue;
                        }

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
}

public class OutputRouteStorage
{
    private Dictionary<AbstractNodeInstance, EnergyRoute> savedRoutes { get; } = new();

    public void addRoute(AbstractNodeInstance node, EnergyRoute route)
    {
        savedRoutes[node] = route;
    }

    public EnergyRoute getRoute(AbstractNodeInstance node)
    {
        return savedRoutes[node];
    }
}

public class EnergyRoute
{
    private List<EnergyStep> steps { get; } = new();

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

    private Connection<AbstractNodeInstance, AbstractNodeInstance> connection { get; }

    // true -> 1 to 2 else 2 to 1
    private bool reverseDirection { get; }
}