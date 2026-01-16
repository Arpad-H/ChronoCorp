using Unity.VisualScripting;
using Util;

namespace Backend.Simulation.World
{
    
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

        public abstract void onActivation();
        
        public abstract void onDeactivation();
    }
    
    public enum StabilityMalusType
    {
        DRAINRATE_INCREASE = 0,
        NODE_SPAWNRATE_INCREASE = 1,
        STABILITY_DECREASE = 2
    }
    
    public class StabilityMalusRegistration
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

    public class NodeDrainMalus : StabilityMalus
    {
        private const double DRAINRATE_INCREASE = 0.5;

        private int change;
        public NodeDrainMalus() : base(StabilityMalusType.DRAINRATE_INCREASE, "NodeDrain")
        {
        }

        public override void tick(long tick)
        {
        }

        public override void onActivation()
        {
            int addedAmount = (int)(BalanceProvider.Balance.nodeDrainHealthEveryNTicks * DRAINRATE_INCREASE);
            change = addedAmount;
            BalanceProvider.Balance.nodeDrainHealthEveryNTicks += addedAmount;
        }

        public override void onDeactivation()
        {
            BalanceProvider.Balance.nodeDrainHealthEveryNTicks -= change;
        }
    }
    
    public class NodeSpawnMalus : StabilityMalus
    {
        private const double SPAWNRATE_INCREASE = 0.25;

        private int change;
        public NodeSpawnMalus() : base(StabilityMalusType.NODE_SPAWNRATE_INCREASE, "NodeSpawn")
        {
        }

        public override void tick(long tick)
        {
        }

        public override void onActivation()
        {
            int addedAmount = (int)(BalanceProvider.Balance.nodeSpawnIntervalPerSecond * SPAWNRATE_INCREASE);
            change = addedAmount;
            BalanceProvider.Balance.nodeSpawnIntervalPerSecond -= addedAmount;
        }

        public override void onDeactivation()
        {
            BalanceProvider.Balance.nodeSpawnIntervalPerSecond += change;
        }
    }
    
    public class StabilityDecrease : StabilityMalus
    {
        private const float NEW_DECREASE = 1;
        private float oldDecrease;
        private readonly StabilityBar stabilityBar;

        public StabilityDecrease(StabilityBar stabilityBar) : base(StabilityMalusType.STABILITY_DECREASE,"StabilityDecrease")
        {
            this.stabilityBar = stabilityBar;
        }

        public override void tick(long tick)
        {
        }

        public override void onActivation()
        {
            oldDecrease = BalanceProvider.Balance.stabilityDecreaseValue;
            stabilityBar.valueDecreasePerTick =  NEW_DECREASE;
        }

        public override void onDeactivation()
        {
            stabilityBar.valueDecreasePerTick = oldDecrease;
        }
    }

}