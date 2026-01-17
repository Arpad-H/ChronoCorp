using System.Collections.Generic;
using Backend.Simulation.Energy;
using Backend.Simulation.World;
using UnityEngine;

namespace NodeBase
{
    public class BlockadeNodeInstance : AbstractNodeInstance, NodeWithConnections
    {
        public List<Connection> Connections;
        public BlockadeNodeInstance(Vector2 pos) : base(pos, NodeType.BLACK_HOLE)
        {
            Connections = new List<Connection>();
        }

        public override void Tick(long tickCount, SimulationStorage storage)
        {
            
        }

        public override void onReceiveEnergyPacket(long tickCount, EnergyPacket energyPacket, SimulationStorage storage)
        {
            
        }

        public override bool onRelayEnergyPacket(long tickCount, EnergyPacket energyPacket, SimulationStorage storage)
        {
            return false;
        }

        public List<Connection> getConnections()
        {
            return Connections;
        }
    }
}