using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class EnergyPacketVisualizer : MonoBehaviour
{
    public static EnergyPacketVisualizer Instance;
    private ObjectPool<EnergyPacketVisual> pool;
    public GameObject prefab;
    public Conduit conduit;
    private void Awake()
    {
        Instance = this;
        pool = new ObjectPool<EnergyPacketVisual>(
            createFunc: CreateItem,
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroyItem,
            collectionCheck: true, // helps catch double-release mistakes
            defaultCapacity: 10,
            maxSize: 100
        );
    }

    void Update()
    {
        // Press Space to spawn one pooled object 
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EnergyPacketVisual ePVisual = pool.Get();
            ePVisual.SetConduit(conduit);
        }
    }
    
    private EnergyPacketVisual CreateItem()
    {
        GameObject ePVisual = Instantiate(prefab);
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
    
}