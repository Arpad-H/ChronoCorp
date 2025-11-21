using System.Collections.Generic;
using System.Numerics;

namespace NodeBase
{
    /**
     * Abstract parent class of node instances
     */
    public abstract class AbstractNodeInstance
    {
        protected AbstractNodeInstance(Vector2 pos, NodeType nodeType)
        {
            NodeType = nodeType;
            Pos = pos;
        }

        private Vector2 Pos { get; set; }
        protected NodeType NodeType { get; }
    }

    /**
     * Refers to generator objects that exist multiple times in the simulation
     */
    public class GeneratorInstance : AbstractNodeInstance
    {
        public GeneratorInstance(Vector2 pos) : base(pos, NodeType.GENERATOR)
        {
            AvailableOutputs = new List<Output>();
        }

        public List<Output> AvailableOutputs { get; }
    }

    /**
 * Refers to time ripple objects that exist multiple times in the simulation
 */
    public class TimeRippleInstance : AbstractNodeInstance, NodeWithConnections
    {
        public TimeRippleInstance(Vector2 pos, EnergyType energyType) : base(pos, NodeType.TIME_RIPPLE)
        {
            Connections = new List<Connection<AbstractNodeInstance, AbstractNodeInstance>>();
            EnergyType = energyType;
        }

        public List<Connection<AbstractNodeInstance, AbstractNodeInstance>> Connections;
        public EnergyType EnergyType { get; set; }
        public List<Connection<AbstractNodeInstance, AbstractNodeInstance>> getConnections()
        {
            return Connections;
        }
    }

    public interface NodeWithConnections
    {
        public List<Connection<AbstractNodeInstance, AbstractNodeInstance>> getConnections();
    }

    /**
     * An output of a generator. A generator can have n outputs
     */
    public class Output
    {
        public GeneratorInstance Parent { get; set; }
        public GeneratorToRippleConnection Connection { get; set; }
    }

    /**
     * Only exists once for each type. Contains basic information about a node which every node of this type share.
     */
    public class NodeType
    {
        public static NodeType GENERATOR = new(Shape.CIRCLE);
        public static NodeType TIME_RIPPLE = new(Shape.SQUARE);

        public NodeType(Shape shape)
        {
            Shape = shape;
        }

        private Shape Shape { get; }
    }

    /**
     * Abstract connection type that defines a start and an end type
     */
    public abstract class Connection<NODE1, NODE2>
        where NODE1 : AbstractNodeInstance
        where NODE2 : AbstractNodeInstance
    {
        protected Connection(NODE1 node1, NODE2 node2)
        {
            this.node1 = node1;
            this.node2 = node2;
        }

        public NODE1 node1 { get; }
        public NODE2 node2 { get; }
    }

    public class GeneratorToRippleConnection : Connection<GeneratorInstance, TimeRippleInstance>
    {
        public GeneratorToRippleConnection(GeneratorInstance node1, TimeRippleInstance node2) : base(node1, node2)
        {
        }
    }

    public class RippleToRippleConnection : Connection<TimeRippleInstance, TimeRippleInstance>
    {
        public RippleToRippleConnection(TimeRippleInstance node1, TimeRippleInstance node2) : base(node1, node2)
        {
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
        RED,
        BLUE,
        GREEN
    }
}