using System;
using System.Collections.Generic;
using Backend.Simulation.World;
using NodeBase;
using Util;

namespace Backend.Simulation.Energy
{
    public class EnergyPacket : ITickable
    {
        private float PacketTravelSpeedPerTick = 0.1f;

        private int _currentEdgeIndex;
        private float _travelledOnEdge;

        public bool Delivered;

        public EnergyPacket(
            EnergyType energyType,
            EnergyPacketSpawner source,
            AbstractNodeInstance destination,
            List<EnergyStep> steps)
        {
            Guid = Guid.NewGuid();
            EnergyType = energyType;
            Source = source;
            Destination = destination;
            Steps = steps;
            PacketTravelSpeedPerTick = BalanceProvider.Balance.energyPacketSpeed;
        }

        public Guid Guid { get; }

        // Units traveled along the edge
        public float progressOnEdge { get; set; }

        public EnergyType EnergyType { get; }

        public EnergyPacketSpawner Source { get; set; }
        public AbstractNodeInstance Destination { get; set; }
        private List<EnergyStep> Steps { get; }

        public void Tick(long tick, SimulationStorage storage)
        {
            if (Delivered) return;
            _travelledOnEdge += PacketTravelSpeedPerTick;
            progressOnEdge = _travelledOnEdge / currentStep().connection.length;
            if (progressOnEdge < 1.0f) return;

            progressOnEdge = 0;
            _travelledOnEdge = 0;

            if (_currentEdgeIndex+1 >= Steps.Count)
            {
                Delivered = true;
                Destination.onReceiveEnergyPacket(tick, this, storage);
            }
            else
            {
                if (!currentStep().getStart().onRelayEnergyPacket(tick, this, storage) || !currentStep().getEnd().onRelayEnergyPacket(tick, this, storage))
                {
                    Delivered = true;
                }
            }
            
            _currentEdgeIndex++;
        }

        public EnergyStep currentStep()
        {
            return Steps[_currentEdgeIndex];
        }
    }
}