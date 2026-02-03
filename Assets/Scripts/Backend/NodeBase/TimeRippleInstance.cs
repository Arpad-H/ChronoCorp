using System.Collections.Generic;
using Backend.Simulation.Energy;
using Backend.Simulation.World;
using UnityEngine;
using Util;

namespace NodeBase
{
    /**
 * Refers to time ripple objects that exist multiple times in the simulation
 */
    public class TimeRippleInstance : AbstractNodeInstance, NodeWithConnections
    {
        public List<Connection> Connections;
        public int minStability {get; set;}
        public int maxStability  {get; set;}
        public int currentStability  {get; set;}
        public long lastEnergyDrainTick {get; set;}
        public long lastStabilityDrainTick {get; set;}

        public TimeRippleInstance(Vector2 pos, EnergyType energyType) : base(pos, NodeType.TIME_RIPPLE)
        {
            Connections = new List<Connection>();
            EnergyType = energyType;
            minStability = BalanceProvider.Balance.nodeMinHp;
            maxStability = BalanceProvider.Balance.nodeMaxHp;
            currentStability = maxStability;
        }

        public EnergyType EnergyType { get; set; }

        public List<Connection> getConnections()
        {
            return Connections;
        }

        public EnergyType getAcceptedEnergyType()
        {
            return EnergyType;
        }

        public override void Tick(long tickCount, SimulationStorage storage)
        {
            if (tickCount - lastEnergyDrainTick >= BalanceProvider.Balance.nodeDrainHealthEveryNTicks)
            {
                currentStability -= BalanceProvider.Balance.nodeHealthDrain;
                if (currentStability < minStability)
                {
                    currentStability = minStability;
                }
                storage.Frontend.onNodeHealthChange(guid, minStability, maxStability, currentStability);
                lastEnergyDrainTick = tickCount;
            }

            if (tickCount - lastStabilityDrainTick >= BalanceProvider.Balance.stabilityDecreaseTicks)
            {
                var currentEnergyThreshold = currentStability * 1f / maxStability;
                var maximumGlobalStabilityGain = BalanceProvider.Balance.baseStabilityDecreasePerNode / BalanceProvider.Balance.nodeStableThresholdPercentage;
                float currentStabilityGain;
                
                if (currentEnergyThreshold > BalanceProvider.Balance.nodeStableThresholdPercentage)
                {
                    currentStabilityGain = BalanceProvider.Balance.stabilityIncreasePerTick;
                    storage.StabilityBar.increaseStability(currentStabilityGain, storage);
                }else
                {
                    currentStabilityGain = BalanceProvider.Balance.baseStabilityDecreasePerNode;
                    storage.StabilityBar.decreaseStability(currentStabilityGain, storage);
                }
              
                lastStabilityDrainTick = tickCount;
            }
        }

        public override void onReceiveEnergyPacket(long tickCount, EnergyPacket energyPacket, SimulationStorage storage)
        {
            currentStability += BalanceProvider.Balance.energyPacketRechargeAmount;
            if (currentStability > maxStability)
            {
                currentStability = maxStability;
            }
            storage.Frontend.onNodeHealthChange(guid, minStability, maxStability, currentStability);
        }
    }
}