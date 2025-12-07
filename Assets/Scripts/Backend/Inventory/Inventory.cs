using System.Collections.Generic;
using UnityEngine;
using Backend.Inventory;

public class Inventory
{
    public Dictionary<Nodes, int> nodesAvailable = new Dictionary<Nodes, int>();
    
    
    public Inventory()
    {
        foreach (Nodes key in InventoryConfig.startConf.Keys)
        {
            nodesAvailable.Add(key, InventoryConfig.startConf[key]);
        }
    }


    public bool placeNormalConnection()
    {
        if (nodesAvailable[Nodes.NormalConnection] > 0)
        {
            nodesAvailable[Nodes.NormalConnection]--;
            return true;
        }

        return false;
    }

    public void removeNormalConnection()
    {
        nodesAvailable[Nodes.NormalConnection]++;
    }

    public bool placeGenerator()
    {
        if (nodesAvailable[Nodes.Generator] > 0)
        {
            nodesAvailable[Nodes.Generator]--;
            return true;
        }
        return false;
    }

    public void removeGenerator()
    {
        nodesAvailable[Nodes.Generator]++;
    }
}

