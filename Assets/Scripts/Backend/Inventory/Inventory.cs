using System.Collections.Generic;
using Interfaces;
using Util;

namespace Backend.Inv
{
    public static class InventoryConfig

    {
        public const int STARTNUMBER_OF_CONNECTIONS = int.MaxValue;

        public static Dictionary<InventoryItem, int> startConf = new()
        {
            [InventoryItem.NORMALCONNECTION] = STARTNUMBER_OF_CONNECTIONS,
            [InventoryItem.GENERATOR] = BalanceProvider.Balance.initialGeneratorCount,
            [InventoryItem.UPGRADE_CARD] = BalanceProvider.Balance.initialUpgradeCardCount,
            [InventoryItem.BRIDGE] = BalanceProvider.Balance.initialBridgeCount,
            [InventoryItem.PAUSE_POWERUP] = BalanceProvider.Balance.initialPausePowerupCount
        };
    }
    public class Inventory
    {
        public Dictionary<InventoryItem, int> inventoryItemsAvailable = new();
        public static GameFrontendManager Instance;
        
        public Inventory()
        {
            foreach (var key in InventoryConfig.startConf.Keys) inventoryItemsAvailable.Add(key, InventoryConfig.startConf[key]);
        }

        private void place(InventoryItem item)
        {
            inventoryItemsAvailable[item]--;
        }

        private void remove(InventoryItem item)
        {
            inventoryItemsAvailable[item]++;
        }


        public int getAmountPlaceable(InventoryItem item)
        {
            if (inventoryItemsAvailable.ContainsKey(item)) return inventoryItemsAvailable[item];

            return 0;
        }

        public bool canPlaceNormalConnection()
        {
            return inventoryItemsAvailable[InventoryItem.NORMALCONNECTION] > 0;
        }

        public bool placeNormalConnection()
        {
            if (canPlaceNormalConnection())
            {
                place(InventoryItem.NORMALCONNECTION);
                return true;
            }

            return false;
        }

        public void removeNormalConnection()
        {
            remove(InventoryItem.NORMALCONNECTION);
        }

        public bool placeGenerator()
        {
            if (canPlaceGenerator())
            {
                place(InventoryItem.GENERATOR);
                return true;
            }

            return false;
        }

        public void removeGenerator()
        {
            remove(InventoryItem.GENERATOR);
        }

        public bool canPlaceGenerator()
        {
            return inventoryItemsAvailable[InventoryItem.GENERATOR] > 0;
        }

        public int addItem(InventoryItem item, int amount)
        {
            if (!inventoryItemsAvailable.ContainsKey(item))
                inventoryItemsAvailable[item] = 0;
            inventoryItemsAvailable[item] += amount;
            return inventoryItemsAvailable[item];
        }
    }
}