using System.Collections.Generic;
using Backend.Simulation.Energy;
using Backend.Simulation.World;
using JetBrains.Annotations;
using UnityEngine;

namespace NodeBase
{
    /**
     * Refers to generator objects that exist multiple times in the simulation
     */
    public class GeneratorInstance : AbstractNodeInstance, EnergyPacketSpawner
    {
        public GeneratorInstance(Vector2 pos, int amountInitialOutputs) : base(pos, NodeType.GENERATOR)
        {
            totalOutputs = new List<Output>(amountInitialOutputs);
            for (var i = 0; i < amountInitialOutputs; i++) totalOutputs.Add(new Output());
        }

        public List<Output> totalOutputs { get; }

        [CanBeNull]
        public Output findFreeOutput()
        {
            foreach (var availableOutput in totalOutputs)
                if (availableOutput.Connection == null)
                    return availableOutput;

            return null;
        }

        public Output findOutputWithConnection(Connection connection)
        {
            foreach (var availableOutput in totalOutputs)
                if (availableOutput.Connection == connection)
                    return availableOutput;

            return null;
        }

        public bool alreadyConnectedTo(AbstractNodeInstance anyNode)
        {
            foreach (var availableOutput in totalOutputs)
                if (availableOutput.Connection?.isPartOfConnection(anyNode) ?? false)
                    return true;

            return false;
        }

        public override void Tick(long tickCount, SimulationStorage storage)
        {
            foreach (var availableOutput in totalOutputs) EnergyScheduler.tick(tickCount, availableOutput, storage);
        }
    }
}