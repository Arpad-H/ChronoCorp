using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Blockade : NodeVisual
{
    public Image hpImage; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpdateHealthBar(float hp)
    {
        hpImage.fillAmount = hp;
    }
    public override void RemoveConnectedConduit(ConduitVisual conduitVisual)
    {
        
        foreach (var dir in isDirectionOccupied)
        {
            if (dir.Value == conduitVisual)
            {
                isDirectionOccupied[dir.Key] = null;
                break;
            }
        }
        connectedConduits.Remove(conduitVisual);
        if (connectedConduits.Count == 0) ;
    }

    public override void AddConnectedConduit(ConduitVisual conduitVisual,Direction dir)
    {
        isDirectionOccupied[dir] = conduitVisual;
        connectedConduits.Add(conduitVisual);
    }

    private void OnDestroy()
    {
        foreach (var conduit in connectedConduits.ToList())
        {
            conduit.DeleteConduit();
        }
    }
}
