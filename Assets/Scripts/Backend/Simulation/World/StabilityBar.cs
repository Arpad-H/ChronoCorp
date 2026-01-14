using System.Collections.Generic;
using Interfaces;
using NodeBase;
using UnityEngine;
using Util;

namespace Backend.Simulation.World
{
    public class StabilityBar : ITickable
    {
        public long MALUS_TICK_SPEED = BalanceProvider.Balance.stabilityDecreaseTicks;
        public float valueDecreasePerTick = BalanceProvider.Balance.stabilityDecreaseValue;
        private readonly List<StabilityMalus> _activeMalusses = new();

        public readonly Dictionary<StabilityMalusType, StabilityMalusRegistration> _malusRegistrations = new();
        private long _lastTick;

        public StabilityBar(int maxValue, int minValue, int currentValue)
        {
            this.maxValue = maxValue;
            this.minValue = minValue;
            this.currentValue = currentValue;
            
            AddMalus((int)(BalanceProvider.Balance.malusThresholds[0] * maxValue),new NodeDrainMalus());
            AddMalus((int)(BalanceProvider.Balance.malusThresholds[1] * maxValue),new NodeSpawnMalus());
            AddMalus((int)(BalanceProvider.Balance.malusThresholds[2] * maxValue),new StabilityDecrease(this));
        }

        public int maxValue { get; set; }
        public int minValue { get; set; }
        public float currentValue { get; set; }

        public void Tick(long tickCount, SimulationStorage storage)
        {
            if (tickCount - _lastTick < MALUS_TICK_SPEED) return;
            _lastTick = tickCount;

            var amountRipplesInSimulation = storage.nodeTypeToNodesMapping.ContainsKey(NodeType.TIME_RIPPLE)
                ? storage.nodeTypeToNodesMapping[NodeType.TIME_RIPPLE].Count
                : 0;
            valueDecreasePerTick = BalanceProvider.Balance.stabilityDecreaseValue +
                                   amountRipplesInSimulation * BalanceProvider.Balance.baseStabilityDecreasePerNode;

            decreaseStability(valueDecreasePerTick, storage);

            UpdateActiveMalusses(storage.Frontend);

            foreach (var malus in _activeMalusses) malus.tick(tickCount);

            if (currentValue <= minValue)
            {
                storage.Frontend.GameOver("Stability bar is zero! You have lost the game.");
            }


        }

        public void decreaseStability(float value, SimulationStorage storage)
        {
            updateStability(currentValue - value, storage);
        }

        public void increaseStability(float value, SimulationStorage storage)
        {
            updateStability(currentValue + value, storage);
        }

        public void updateStability(float newStabilityValue, SimulationStorage storage)
        {
            var oldValue = currentValue;
            setStability(newStabilityValue);
            var newValue = currentValue;

            if (oldValue - newValue <= float.Epsilon)
            {
                return;
            }

            storage.Frontend.OnStabilityBarUpdate(minValue, maxValue, (int)currentValue);
        }

        private void setStability(float value)
        {
            if (value < minValue) currentValue = minValue;
            else if (value > maxValue) currentValue = maxValue;
            else currentValue = value;
        }

        public void AddMalus(int threshold, StabilityMalus malus)
        {
            _malusRegistrations[malus.StabilityMalusType] = new StabilityMalusRegistration(threshold, malus);
        }

        public bool IsMalusActiveByType(StabilityMalusType malusType)
        {
            if (_malusRegistrations.TryGetValue(malusType, out var registration)) return registration.IsActive;

            return false;
        }

        public int getActivationThreshold(StabilityMalusType malusType)
        {
            if (_malusRegistrations.TryGetValue(malusType, out var registration))
                return registration.ActivationThreshold;

            throw new KeyNotFoundException(
                $"No activation threshold registered for malus type {malusType}");
        }

        private void UpdateActiveMalusses(IFrontend frontend)
        {
            foreach (var registration in _malusRegistrations.Values)
            {
                var shouldBeActive = currentValue <= registration.ActivationThreshold;

                if (shouldBeActive && !registration.IsActive)
                {
                    registration.IsActive = true;
                    _activeMalusses.Add(registration.Malus);
                    registration.Malus.onActivation();
                    frontend.OnActivateStabilityMalus(registration.Malus.StabilityMalusType);
                }
                else if (!shouldBeActive && registration.IsActive)
                {
                    registration.IsActive = false;
                    _activeMalusses.Remove(registration.Malus);
                    registration.Malus.onDeactivation();
                    frontend.OnDeactivateStabilityMalus(registration.Malus.StabilityMalusType);
                }
            }
        }
    }
}