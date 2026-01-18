using Backend.Simulation.World;
using NodeBase;
using Util;

namespace Backend.Simulation.Energy
{
    public class EnergyScheduler
    {

        // For one output try to spawn a new packet via cooldown.
        public static void tick(long currentTick, Output output, SimulationStorage storage)
        {
            var tickCooldownOutputs = (int)(SimulationStorage.TICKS_PER_SECOND * BalanceProvider.Balance.energyPacketSpawnIntervalPerSecond);
            output.RouteStorage ??= EnergyRouter.createEnergyRoute(output);

            if (output.RouteStorage.savedRoutes == null || output.RouteStorage.savedRoutes.Count == 0) return;
            var last = output.lastGenerationTick;

            if (currentTick - last < tickCooldownOutputs) return;

            var nextIndex = output.targetIndex++ % output.RouteStorage.savedRoutes.Count;
            var nextRoute = output.RouteStorage.orderedListOfRoutes[nextIndex];

            var stepsInRoute = nextRoute.steps.Count;
            if (stepsInRoute == 0) return;
            var startStep = nextRoute.steps[0];
            var endStep = nextRoute.steps[stepsInRoute - 1];

            var newEnergyPacket = new EnergyPacket(
                ChooseEnergyType(endStep.getEnd()),
                startStep.getStart() as EnergyPacketSpawner,
                endStep.getEnd(),
                nextRoute.steps
            );

            storage.registerEnergyPacket(newEnergyPacket);
            output.lastGenerationTick = currentTick;
        }

        /**
         * Used to determine which energy type to select for a new energy packet based on the energy type a node accepts.
         */
        private static EnergyType ChooseEnergyType(AbstractNodeInstance anyNode)
        {
            if (anyNode is TimeRippleInstance ripple) return ripple.EnergyType;
            return EnergyType.WHITE;
        }
    }
}