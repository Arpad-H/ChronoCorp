using System;
using Backend.Simulation.Energy;
using Backend.Simulation.World;
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
            guid = Guid.NewGuid();
            NodeType = nodeType;
            Pos = pos;
        }

        public Guid guid { get; }
        public Vector2 Pos { get; set; }
        public NodeType NodeType { get; }
        public TimeSlice currentTimeSlice { get; set; }
        public abstract void Tick(long tickCount, SimulationStorage storage);

        /**
         * Called when a node receives an energy packet.
         */
        public virtual void onReceiveEnergyPacket(long tickCount, EnergyPacket energyPacket, SimulationStorage storage)
        {
            
        }

        /**
         * Returns true if the relay works. Otherwise, it gets cancelled.
         */
        public virtual bool onRelayEnergyPacket(long tickCount, EnergyPacket energyPacket, SimulationStorage storage)
        {
            return true;
        }
    }
}