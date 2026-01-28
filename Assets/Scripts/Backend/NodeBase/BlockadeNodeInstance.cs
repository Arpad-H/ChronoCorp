using System.Collections.Generic;
using Backend.Simulation.Energy;
using Backend.Simulation.World;
using UnityEngine;
using Util;

namespace NodeBase
{
    public class BlockadeNodeInstance : AbstractNodeInstance, NodeWithConnections
    {
        public List<Connection> Connections;
        private long lastDrain;
        public int energyConsumed = 0;
        private int energyToBeDestroyed = BalanceProvider.Balance.blockadeEnergyPacketConsumeAmount;
        public BlockadeNodeInstance(Vector2 pos) : base(pos, NodeType.BLOCKADE)
        {
            Connections = new List<Connection>();
        }

        public override void Tick(long tickCount, SimulationStorage storage)
        {
            energyToBeDestroyed = BalanceProvider.Balance.blockadeEnergyPacketConsumeAmount;
        }

        public override void onReceiveEnergyPacket(long tickCount, EnergyPacket energyPacket, SimulationStorage storage)
        {
            energyConsumed += energyToBeDestroyed;
            storage.Frontend.onNodeHealthChange(guid, 0, energyToBeDestroyed, energyConsumed);
            Debug.Log("Blockade received energy ");
            if (energyConsumed >= energyToBeDestroyed)
            {
                storage.deleteNode(guid);
                storage.Frontend.DeleteNode(guid);
            }
        }

        public override bool onRelayEnergyPacket(long tickCount, EnergyPacket energyPacket, SimulationStorage storage)
        {
            Debug.Log("Blockade blocked an energy packet");
            return false;
        }

        public List<Connection> getConnections()
        {
            return Connections;
        }

        public EnergyType getAcceptedEnergyType()
        {
            return EnergyType.WHITE;
        }
    }
}