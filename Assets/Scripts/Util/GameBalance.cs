using UnityEngine;

namespace Util
{
    [CreateAssetMenu(menuName = "Game/Balance")]
    public class GameBalance: ScriptableObject
    {
        [Header("Energy Packet Settings")]
        public float energyPacketSpeed;
        public float energyPacketSpawnIntervalPerSecond;
        public int energyPacketRechargeAmount;
        
        [Header("Node Settings")]
        public int nodeMaxHp;
        public int nodeMinHp;
        public int nodeDrainRate;
        public int nodeDrainTicks;
        public float nodeSpawnIntervalPerSecond;
        public float noodeBlinkThreshhold;
        
        [Header("Layer Settings")]
        [Tooltip("Ticks. For example 3000 ticks at 50 TPS = 60 seconds")]
        public int layerDuplicationTime;
        [Tooltip("Number of Cells in X and Y direction per layer")]
        public Vector2Int layerGridCellCount;
        
        
        [Header("Inventory and Item Settings")]
        public int initialGeneratorCount;
        
        [Header("Stability Bar Settings")]
        public int stabilityMaxValue;
        public int stabilityMinValue;
        public float stabilityDecreaseValue;
        public int stabilityDecreaseTicks;
        [Tooltip("Index matches StabilityMalusType enum order. Index 0 -> MALUS1, index 1 -> MALUS2, etc.")]
        [Range(0f, 1f)]
        public float[] malusThresholds;
        
        [Header("Node Stability Contribution")]
        [Tooltip("How much stability this node drains by existing (base value=")]
        public float baseStabilityDecreasePerNode;
        [Tooltip("How much stability this node drains by existing (base value=")]
        [Min(0)]
        public float nodeStableThresholdPercentage;
        
        [Header("Camera Settings")]
        public float cameraZoomSpeed;
        public float cameraPanSpeed;
        

        [Tooltip("Min and Max Camera Y for spiral Mode")]
        public Vector2 spiralGridminMaxCameraY;
    }
}