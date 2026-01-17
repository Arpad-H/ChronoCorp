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
        public long energyConsumed = 0;
        private long energyToBeDestroyed = 100;
        public BlackHoleInstance(Vector2 pos) : base(pos, NodeType.BLACK_HOLE)
        {
            Connections = new List<Connection>();
        }

        public override void Tick(long tickCount, SimulationStorage storage)
        {
            if (lastDrain - tickCount < BalanceProvider.Balance.blackHoleStabilityDrainRate)
            {
                return;
            }
            storage.StabilityBar.decreaseStability(BalanceProvider.Balance.blackHoleDrainEveryNTicks, storage);
        }

        public override void onReceiveEnergyPacket(long tickCount, EnergyPacket energyPacket, SimulationStorage storage)
        {
            energyConsumed += energyToBeDestroyed;
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