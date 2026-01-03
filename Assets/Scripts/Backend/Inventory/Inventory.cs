using System.Collections.Generic;
using Interfaces;

namespace Backend.Inv
{
    public class Inventory
    {
        public Dictionary<NodeDTO, int> nodesAvailable = new();


        public Inventory()
        {
            foreach (var key in InventoryConfig.startConf.Keys) nodesAvailable.Add(key, InventoryConfig.startConf[key]);
        }

        private void place(NodeDTO node)
        {
            nodesAvailable[node]--;
        }

        private void remove(NodeDTO node)
        {
            nodesAvailable[node]++;
        }


        public int getAmountPlaceable(NodeDTO nodeDTO)
        {
            if (nodesAvailable.ContainsKey(nodeDTO)) return nodesAvailable[nodeDTO];

            return 0;
        }

        public bool canPlaceNormalConnection()
        {
            return nodesAvailable[NodeDTO.NORMALCONNECTION] > 0;
        }

        public bool placeNormalConnection()
        {
            if (canPlaceNormalConnection())
            {
                place(NodeDTO.NORMALCONNECTION);
                return true;
            }

            return false;
        }

        public void removeNormalConnection()
        {
            remove(NodeDTO.NORMALCONNECTION);
        }

        public bool placeGenerator()
        {
            if (canPlaceGenerator())
            {
                place(NodeDTO.GENERATOR);
                return true;
            }

            return false;
        }

        public void removeGenerator()
        {
            remove(NodeDTO.GENERATOR);
        }

        public bool canPlaceGenerator()
        {
            return nodesAvailable[NodeDTO.GENERATOR] > 0;
        }
    }

    public static class InventoryConfig

    {
        public const int STARTNUMBER_OF_CONNECTIONS = int.MaxValue;
        public const int STARTNUMBER_OF_GENERATORS = 3;

        public static Dictionary<NodeDTO, int> startConf = new()
        {
            [NodeDTO.NORMALCONNECTION] = STARTNUMBER_OF_CONNECTIONS,
            [NodeDTO.GENERATOR] = STARTNUMBER_OF_GENERATORS
        };
    }
}