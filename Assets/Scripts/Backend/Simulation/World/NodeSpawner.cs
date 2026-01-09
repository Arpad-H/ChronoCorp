using System;
using System.Linq;
using NodeBase;
using UnityEngine;
using Util;
using Random = Unity.Mathematics.Random;

namespace Backend.Simulation.World
{
    public class NodeSpawner : ITickable
    {
        private int TICK_SPAWN_COOLDOWN;
        private Random _random;
        private TimeSlice _timeSlice;
        private long lastSpawnTick;

        public NodeSpawner(TimeSlice timeSlice)
        {
            TICK_SPAWN_COOLDOWN = (int)(SimulationStorage.TICKS_PER_SECOND * BalanceProvider.Balance.nodeSpawnIntervalPerSecond);
            _timeSlice = timeSlice;
            lastSpawnTick = (long)(-TICK_SPAWN_COOLDOWN * 0.8);
        }

        public void Tick(long tickCount, SimulationStorage storage)
        {
            if (tickCount - lastSpawnTick <= TICK_SPAWN_COOLDOWN) return;
            
            _random.InitState(storage.getTickSeed(tickCount));
            var energyTypeOfNewNode = determineEnergyTypeForSpawning(storage);

            if (energyTypeOfNewNode != null)
            {
                _timeSlice.TimeSliceGrid.TryGetRandomEmptyCell(_random, out var cell);
                _timeSlice.spawnRipple(cell, (EnergyType)energyTypeOfNewNode, out var newTimeRipple);
                lastSpawnTick = tickCount;
                Debug.Log("Auto generated new time ripple with "+energyTypeOfNewNode+" energy at "+cell);
                storage.Frontend.PlaceNodeVisual(newTimeRipple.guid,newTimeRipple.NodeType.NodeDTO, _timeSlice.SliceNumber, cell, (EnergyType)energyTypeOfNewNode);
            }
        }

        private EnergyType? determineEnergyTypeForSpawning(SimulationStorage storage)
        {
            var energyTypesToChooseFrom = storage.getEnergyTypesInSimulation();
            energyTypesToChooseFrom.Remove(EnergyType.WHITE);

            var amountOutputsAvailable = storage.getAmountOutputsTotal();
            if (amountOutputsAvailable == 0) return null;
            var amountEnergyTypesInSimulation = storage.getAmountDifferentEnergyTypesInSimulation();
            
            if (amountOutputsAvailable > amountEnergyTypesInSimulation)
            {
                var amountNewEnergyTypesAllowed = amountOutputsAvailable - amountEnergyTypesInSimulation;
                foreach (EnergyType energyTypeCandidate in Enum.GetValues(typeof(EnergyType)))
                {
                    if (amountNewEnergyTypesAllowed <= 0) break;
                    if (energyTypesToChooseFrom.Contains(energyTypeCandidate) || energyTypeCandidate.Equals(EnergyType.WHITE)) continue;
                    energyTypesToChooseFrom.Add(energyTypeCandidate);
                    amountNewEnergyTypesAllowed--;
                }
            }

            var chooseList = energyTypesToChooseFrom.ToList();
            
            if (chooseList.Count == 0)
            {
                return null;
            }
            _random.NextInt(0, chooseList.Count);
            return chooseList[_random.NextInt(0, chooseList.Count)];
        }
    }
}