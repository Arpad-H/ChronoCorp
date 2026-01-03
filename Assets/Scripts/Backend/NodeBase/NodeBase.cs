using System.Collections.Generic;
using Backend.Simulation.Energy;
using Backend.Simulation.World;
using Interfaces;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace NodeBase
{
    /**
     * Abstract parent class of node instances
     */
    public abstract class AbstractNodeInstance : ITickable
    {
        protected AbstractNodeInstance(Vector2 pos, NodeType nodeType)
        {
            guid = GUID.Generate();
            NodeType = nodeType;
            Pos = pos;
        }

        public GUID guid { get; }
        public Vector2 Pos { get; set; }
        public NodeType NodeType { get; }
        public abstract void Tick(long tickCount, SimulationStorage storage);
    }

    /**
     * Refers to generator objects that exist multiple times in the simulation
     */
    public class GeneratorInstance : AbstractNodeInstance, EnergyPacketSpawner
    {
        public GeneratorInstance(Vector2 pos, int amountInitialOutputs) : base(pos, NodeType.GENERATOR)
        {
            AvailableOutputs = new List<Output>(amountInitialOutputs);
            for (var i = 0; i < amountInitialOutputs; i++) AvailableOutputs.Add(new Output());
        }

        public List<Output> AvailableOutputs { get; }

        [CanBeNull]
        public Output findFreeOutput()
        {
            foreach (var availableOutput in AvailableOutputs)
                if (availableOutput.Connection == null)
                    return availableOutput;

            return null;
        }

        public Output findOutputWithConnection(Connection connection)
        {
            foreach (var availableOutput in AvailableOutputs)
                if (availableOutput.Connection == connection)
                    return availableOutput;

            return null;
        }

        public bool alreadyConnectedTo(AbstractNodeInstance anyNode)
        {
            foreach (var availableOutput in AvailableOutputs)
                if (availableOutput.Connection?.isPartOfConnection(anyNode) ?? false)
                    return true;

            return false;
        }

        public override void Tick(long tickCount, SimulationStorage storage)
        {
            foreach (var availableOutput in AvailableOutputs) EnergyScheduler.tick(tickCount, availableOutput, storage);
        }
    }

    /**
 * Refers to time ripple objects that exist multiple times in the simulation
 */
    public class TimeRippleInstance : AbstractNodeInstance, NodeWithConnections
    {
        public const int ENERGY_DRAIN_TICKS = 50 * 5;
        
        public List<Connection> Connections;
        public int minStability {get; set;}
        public int maxStability  {get; set;}
        public int currentStability  {get; set;}
        public long lastDrainTick {get; set;}

        public TimeRippleInstance(Vector2 pos, EnergyType energyType) : base(pos, NodeType.TIME_RIPPLE)
        {
            Connections = new List<Connection>();
            EnergyType = energyType;
            minStability = 0;
            maxStability = 100;
            currentStability = maxStability;
        }

        public EnergyType EnergyType { get; set; }

        public List<Connection> getConnections()
        {
            return Connections;
        }

        public override void Tick(long tickCount, SimulationStorage storage)
        {
            if (tickCount - lastDrainTick < ENERGY_DRAIN_TICKS)
            {
                return;
            }

            currentStability -= 1;
            if (currentStability < minStability)
            {
                currentStability = minStability;
            }
            storage.Frontend.onNodeHealthChange(guid, minStability, maxStability, currentStability);
            lastDrainTick = tickCount;
        }
    }

    public interface NodeWithConnections
    {
        public List<Connection> getConnections();

        public bool HasDirectConnectionTo(AbstractNodeInstance anyNode)
        {
            foreach (var connection in getConnections())
                if (connection.isPartOfConnection(anyNode))
                    return true;

            return false;
        }
    }

    public interface EnergyPacketSpawner
    {
    }

    /**
     * An output of a generator. A generator can have n outputs
     */
    public class Output
    {
        public GeneratorInstance Parent { get; set; }
        public Connection Connection { get; set; }

        public OutputRouteStorage RouteStorage { get; set; }
        public long lastGenerationTick { get; set; }
        public int targetIndex { get; set; }
    }

    /**
     * Only exists once for each type. Contains basic information about a node which every node of this type share.
     */
    public class NodeType
    {
        public static NodeType GENERATOR = new(Shape.CIRCLE, NodeDTO.GENERATOR);
        public static NodeType TIME_RIPPLE = new(Shape.SQUARE, NodeDTO.RIPPLE);

        public readonly Shape Shape; //  private Shape Shape { get; } only allows internal access
        public readonly NodeDTO NodeDTO;

        public NodeType(Shape shape, NodeDTO nodeDTO)
        {
            Shape = shape;
            NodeDTO = nodeDTO;
        }
        
        
    }

    /**
     * Abstract connection type that defines a start and an end type
     */
    public class Connection
    {
        public Connection(AbstractNodeInstance node1, AbstractNodeInstance node2)
        {
            guid = GUID.Generate();
            this.node1 = node1;
            this.node2 = node2;
            var direction = node2.Pos - node1.Pos;
            length = direction.magnitude;
        }

        public GUID guid { get; }

        public AbstractNodeInstance node1 { get; }
        public AbstractNodeInstance node2 { get; }

        public float length { get; }

        public bool isPartOfConnection(AbstractNodeInstance anyNode)
        {
            return anyNode == node1 || anyNode == node2;
        }
    }

    /**
     * Defines the basic shape of a node
     */
    public enum Shape
    {
        CIRCLE,
        SQUARE,
        TRIANGLE
    }

    /**
     * Defines the energy shape a time ripple accepts
     */
    public enum EnergyType
    {
        // Wildcard energy Type. Everytime a node would accept all energy types -> We use white
        WHITE,
        YELLOW,
        RED,
        BLUE,
        GREEN
    }

    public static class EnergyTypeExtensions
    {
        public static Color ToColor(this EnergyType t)
        {
            return t switch
            {
                EnergyType.GREEN => Color.green,
                EnergyType.RED => Color.red,
                EnergyType.BLUE => Color.blue,
                EnergyType.YELLOW => Color.yellow,
                EnergyType.WHITE => Color.white
            };
        }
    }
}