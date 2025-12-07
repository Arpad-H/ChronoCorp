using System.Collections.Generic;
using Interfaces;

namespace Backend.Simulation.World
{
    public class StabilityBar : ITickable
    {
        public const long MALUS_TICK_SPEED = 50;
        public const int valueDecreasePerTick = 5;
        private readonly List<StabilityMalus> _activeMalusses = new();

        private readonly Dictionary<StabilityMalusType, StabilityMalusRegistration> _malusRegistrations = new();
        private long _lastTick;

        public StabilityBar(int maxValue, int minValue, int currentValue)
        {
            this.maxValue = maxValue;
            this.minValue = minValue;
            this.currentValue = currentValue;
        }

        public int maxValue { get; set; }
        public int minValue { get; set; }
        public int currentValue { get; set; }

        public void Tick(long tickCount, SimulationStorage storage)
        {
            if (tickCount - _lastTick < MALUS_TICK_SPEED) return;
            _lastTick = tickCount;

            setStability(currentValue - valueDecreasePerTick);
            storage.Frontend.OnStabilityBarUpdate(minValue, maxValue, currentValue);

            UpdateActiveMalusses(storage.Frontend);

            foreach (var malus in _activeMalusses) malus.tick(tickCount);
        }

        private void setStability(int value)
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
                    frontend.OnActivateStabilityMalus(registration.Malus.StabilityMalusType);
                }
                else if (!shouldBeActive && registration.IsActive)
                {
                    registration.IsActive = false;
                    _activeMalusses.Remove(registration.Malus);
                    frontend.OnDeactivateStabilityMalus(registration.Malus.StabilityMalusType);
                }
            }
        }

        private class StabilityMalusRegistration
        {
            public StabilityMalusRegistration(int activationThreshold, StabilityMalus malus)
            {
                ActivationThreshold = activationThreshold;
                Malus = malus;
            }

            public StabilityMalus Malus { get; }
            public int ActivationThreshold { get; }
            public bool IsActive { get; set; }
        }
    }

    public enum StabilityMalusType
    {
        TIME_RIPPLE_SPAWN_BOOST
    }

    public abstract class StabilityMalus
    {
        public readonly StabilityMalusType StabilityMalusType;

        protected StabilityMalus(StabilityMalusType stabilityMalusType, string malusId)
        {
            StabilityMalusType = stabilityMalusType;
            this.malusId = malusId;
        }

        public string malusId { get; private set; }

        public abstract void tick(long tick);
    }
}