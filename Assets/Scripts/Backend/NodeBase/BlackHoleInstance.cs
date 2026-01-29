using System.Collections.Generic;
using Backend.Simulation.Energy;
using Backend.Simulation.World;
using UnityEngine;
using Util;

namespace NodeBase
{
    public class BlackHoleInstance : AbstractNodeInstance, NodeWithConnections
    {
        public List<Connection> Connections;
        private long lastDrain;
        public int energyConsumed = 0;
        private int energyToBeDestroyed = BalanceProvider.Balance.blackHoleEnergyPacketConsumeAmount;
        public BlackHoleInstance(Vector2 pos) : base(pos, NodeType.BLACK_HOLE)
        {
            Connections = new List<Connection>();
        }

        public override void Tick(long tickCount, SimulationStorage storage)
        {
            if (tickCount - lastDrain < BalanceProvider.Balance.blackHoleDrainEveryNTicks)
            {
                return;
            }
            energyToBeDestroyed = BalanceProvider.Balance.blackHoleEnergyPacketConsumeAmount;

            lastDrain = tickCount;
            storage.StabilityBar.decreaseStability(BalanceProvider.Balance.blackHoleStabilityDrainRate, storage);
        }

        public override void onReceiveEnergyPacket(long tickCount, EnergyPacket energyPacket, SimulationStorage storage)
        {
            energyConsumed += 1;
            storage.Frontend.onNodeHealthChange(guid, 0, energyToBeDestroyed, energyConsumed);
            if (energyConsumed >= energyToBeDestroyed)
            {
                storage.deleteNode(guid);
                storage.Frontend.DeleteNode(guid);
            }
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