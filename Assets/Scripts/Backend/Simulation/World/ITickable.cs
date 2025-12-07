namespace Backend.Simulation.World
{
    public interface ITickable
    {
        public void Tick(long tickCount, SimulationStorage storage);
    }
}