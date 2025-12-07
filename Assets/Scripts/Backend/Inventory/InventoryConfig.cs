using UnityEngine;
using System.Collections.Generic;

namespace Backend.Inventory
{
    public static class InventoryConfig
    
    {
        public const int STARTNUMBER_OF_CONNECTIONS = 15;
        public const int STARTNUMBER_OF_GENERATORS = 5;
        
        public static Dictionary<Nodes, int> startConf = new Dictionary<Nodes, int>
        {
            [Nodes.NormalConnection] = STARTNUMBER_OF_CONNECTIONS,
            [Nodes.Generator] = STARTNUMBER_OF_GENERATORS
        };
    }

    public enum Nodes
    {
        Generator,
        TimeRipple,
        NormalConnection
    }
}