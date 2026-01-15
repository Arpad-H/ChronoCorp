using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Generator : NodeVisual
{
    private int generatorTier = 1;
    public GameObject currentTierObject;
    public GameObject tier1Prefab;
    public GameObject tier2Prefab;
    public GameObject tier3Prefab;
    public GameObject tier4Prefab;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentTierObject = tier1Prefab;
        SetGeneratorTier(1);
    }

    // Update is called once per frame
  
    public override void RemoveConnectedConduit(ConduitVisual conduitVisual)
    {
        connectedConduits.Remove(conduitVisual);
        if (connectedConduits.Count == 0) ;
    }

    public override void AddConnectedConduit(ConduitVisual conduitVisual)
    {
        connectedConduits.Add(conduitVisual);
    }
    public void SetGeneratorTier(int tier)
    {
        Debug.Log("Setting generator tier to " + tier);
        generatorTier = tier;
        currentTierObject.SetActive(false);
        switch (generatorTier)
        {
            case 1:
               currentTierObject = tier1Prefab;
                break;
            case 2:
                currentTierObject = tier2Prefab;
                break;
            case 3:
                currentTierObject = tier3Prefab;
                break;
            case 4:
                currentTierObject = tier4Prefab;
                break;
            default:
                
                break;
        }
        currentTierObject.SetActive(true);
    }

    public void UpgradeNode()
    {
        SetGeneratorTier (generatorTier + 1);
    }
}
