using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace Util
{
    public enum StatType
    {
        // Energy Packet Settings
        EnergyPacketSpeed,
        EnergyPacketSpawnIntervalPerSecond,
        EnergyPacketRechargeAmount,

        // Node Settings
        NodeMaxHp,
        NodeMinHp,
        NodeHealthDrainRate,
        NodeDrainHealthEveryNTicks,
        NodeSpawnIntervalPerSecond,
        LayerModifierToNodeSpawnInterval,
        
        // Node Stability Contribution
        BaseStabilityDecreasePerNode,
        NodeStableThresholdPercentage,

        // Layer Settings
        LayerDuplicationTime,

        // Stability Bar Settings
        StabilityMaxValue,
        StabilityDecreaseValue,
        StabilityDecreaseTicks,
        MalusThreshold0,
        MalusThreshold1,
        MalusThreshold2, 

        
    }
    
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
        [Tooltip("How much health this node drains per tick")]
        public int nodeHealthDrainRate;
        [Tooltip("every n ticks, node drains health")]
        public int nodeDrainHealthEveryNTicks;
        [Tooltip("How many seconds between spawns")]
        public float nodeSpawnIntervalPerSecond;
        [Tooltip("Each layer when spawned adds this in seconds to the node spawn interval")]
        public float layerModifierToNodeSpawnInterval;
        [Tooltip("HP percent Threshold below which node starts to blink")]
        public float nodeBlinkThreshhold;
        [Tooltip("How much stability this blackhole drains per tick")]
        public int blackHoleStabilityDrainRate;
        [Tooltip("every n ticks, black hole drains stability")]
        public int blackHoleDrainEveryNTicks;
        
        [Header("Node Stability Contribution")]
        [Tooltip("How much stability this node drains by existing (base value=")]
        public float baseStabilityDecreasePerNode;
        [Min(0)]
        public float nodeStableThresholdPercentage;
        
        
        [Header("Layer Settings")]
        [Tooltip("Ticks. For example 3000 ticks at 50 TPS = 60 seconds")]
        public int layerDuplicationTime;
        [Tooltip("Maximum number of layers allowed in the game")]
        public int maxLayerCount;
        [Tooltip("Number of Cells in X and Y direction per layer")]
        public Vector2Int layerGridCellCount;
        public int cellSize;
        
        
        [Header("Inventory and Item Settings")]
        public int initialGeneratorCount;
        public int initialUpgradeCardCount;
        public int initialBridgeCount;
        
        
        [Header("Stability Bar Settings")]
        public int stabilityMaxValue;
        public int stabilityMinValue;
        public float stabilityDecreaseValue;
        public int stabilityDecreaseTicks;
        [Tooltip("Index matches StabilityMalusType enum order. Index 0 -> MALUS1, index 1 -> MALUS2, etc.")]
        [Range(0f, 1f)]
        public float[] malusThresholds;
        
        
        [Header("Camera Settings")]
        public float cameraZoomSpeed;
        public float cameraPanSpeed;
        [Tooltip("Min and Max Camera Y Coords. Adjust for zoom levels")]
        public Vector2 spiralGridminMaxCameraY;
        
        [Header("Upgrade Cards Settings")]
        public List<UpgradeCardData> upgradeCards;
        
        
        
        public void Add(StatType type, float amount)
        {
            string fieldName = type.ToString();
            FieldInfo field = typeof(GameBalance).GetField(fieldName, BindingFlags.Public | BindingFlags.Instance  | BindingFlags.IgnoreCase);
            if (field != null)
            {
                if (field.FieldType == typeof(int))
                    field.SetValue(this, (int)field.GetValue(this) + (int)amount);
                else if (field.FieldType == typeof(float))
                    field.SetValue(this, (float)field.GetValue(this) + amount);
            }
            else
            {
                Debug.LogError("Unknown StatType: " + type);
            }
        }
        
        public void Multiply(StatType type, float factor)
        {
            string fieldName = type.ToString();
            FieldInfo field = typeof(GameBalance).GetField(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (field != null)
            {
                if (field.FieldType == typeof(int))
                    field.SetValue(this, (int)(int)field.GetValue(this) * factor);
                else if (field.FieldType == typeof(float))
                    field.SetValue(this, (float)field.GetValue(this) * factor);
            }
        }
    }
}