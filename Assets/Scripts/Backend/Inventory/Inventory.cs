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
            [InventoryItem.UPGRADE_CARD] = BalanceProvider.Balance.initialUpgradeCardCount
        };
    }
    public class Inventory
    {
        public Dictionary<InventoryItem, int> nodesAvailable = new();
        public static GameFrontendManager Instance;
        
        public Inventory()
        {
            foreach (var key in InventoryConfig.startConf.Keys) nodesAvailable.Add(key, InventoryConfig.startConf[key]);
        }

        private void place(InventoryItem item)
        {
            nodesAvailable[item]--;
        }

        private void remove(InventoryItem item)
        {
            nodesAvailable[item]++;
        }


        public int getAmountPlaceable(InventoryItem item)
        {
            if (nodesAvailable.ContainsKey(item)) return nodesAvailable[item];

            return 0;
        }

        public bool canPlaceNormalConnection()
        {
            return nodesAvailable[InventoryItem.NORMALCONNECTION] > 0;
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
            return nodesAvailable[InventoryItem.GENERATOR] > 0;
        }

        public int addItem(InventoryItem item, int amount)
        {
            if (!nodesAvailable.ContainsKey(item))
                nodesAvailable[item] = 0;
            nodesAvailable[item] += amount;
            return nodesAvailable[item];
        }
    }
}