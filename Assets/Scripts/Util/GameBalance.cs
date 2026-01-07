using UnityEngine;

namespace Util
{
    [CreateAssetMenu(menuName = "Game/Balance")]
    public class GameBalance: ScriptableObject
    {
        [Header("Energy Packet Settings")]
        public float energyPacketSpeed;
        public float energyPacketSpawnInterval;
        public float energyPacketRechargeAmount;
        
        [Header("Node Settings")]
        public int nodeMaxHp;
        public int nodeMinHp;
        public float nodeDrainRate;
        
        [Header("Layer Settings")]
        [Tooltip("Ticks. For example 3000 ticks at 50 TPS = 60 seconds")]
        public int layerDuplicationTime;
        
        [Header("Inventory and Item Settings")]
        public int initialGeneratorCount;
        
        [Header("Stability Bar Settings")]
        public float stabilityMaxValue;
        public float[] malusThreshholds;
    }
}