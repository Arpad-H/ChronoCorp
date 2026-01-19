using System;
using System.Collections.Generic;
using Backend.Simulation.Energy;
using Interfaces;
using UnityEngine;

namespace NodeBase
{
    public interface NodeWithConnections
    {
        public List<Connection> getConnections();
        public EnergyType getAcceptedEnergyType();

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
        public static NodeType BLACK_HOLE = new(Shape.CIRCLE, NodeDTO.BLACK_HOLE);
        public static NodeType BLOCKADE = new(Shape.CIRCLE, NodeDTO.BLOCKADE);

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
        public Connection(AbstractNodeInstance node1, AbstractNodeInstance node2, Vector2Int[] cellsOfConnection)
        {
            guid = Guid.NewGuid();
            this.node1 = node1;
            this.node2 = node2;
            length = cellsOfConnection.Length;
        }

        public Guid guid { get; }

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