using System.Collections.Generic;
using Interfaces;
using NodeBase;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

public class EnergyPacketVisualizer : MonoBehaviour
{
    public static EnergyPacketVisualizer Instance;
    public GameObject prefab;
    private ObjectPool<EnergyPacketVisual> pool;
 
    private Dictionary<GUID,EnergyPacketVisual> ePVisuals = new();

    private void Awake()
    {
        Instance = this;
        pool = new ObjectPool<EnergyPacketVisual>(
            CreateItem,
            OnGet,
            OnRelease,
            OnDestroyItem,
            true, // helps catch double-release mistakes
            10,
            100
        );
    }

   
    public void SpawnEnergyPacket(GUID guid,EnergyType energyType) //TODO giga dirty. fix backend reference later
    {
        EnergyPacketVisual ePVisual = pool.Get();
        ePVisual.guid = guid;
        ePVisual.SetEnergyType(energyType);
        ePVisuals.Add(guid,ePVisual);
     
    }

    private EnergyPacketVisual CreateItem()
    {
        var ePVisual = Instantiate(prefab);
        return ePVisual.GetComponent<EnergyPacketVisual>();
    }

    private void OnGet(EnergyPacketVisual ePVisual)
    {
        ePVisual.gameObject.SetActive(true);
    }

    private void OnRelease(EnergyPacketVisual ePVisual)
    {
        ePVisual.gameObject.SetActive(false);
    }

    // Called when the pool decides to destroy an item (e.g., above max size).
    private void OnDestroyItem(EnergyPacketVisual ePVisual)
    {
        Destroy(ePVisual.gameObject);
    }

    public void ReleaseItem(EnergyPacketVisual ePVisual)
    {
        pool.Release(ePVisual);
    }

    public void DeleteEnergyPacket(GUID guid)
    {
        if (!ePVisuals.ContainsKey(guid)) return;
        EnergyPacketVisual ePVisual = ePVisuals[guid];
        ePVisual.RemoveConduitBulge();
        ReleaseItem(ePVisual);
        ePVisuals.Remove(guid);
    }
   
}