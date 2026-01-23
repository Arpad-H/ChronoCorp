using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BlackHole : NodeVisual
{
    private float currentHP = 0;
    public MeshRenderer renderer;
    private Material mat;
    [ColorUsageAttribute(true,true,0f,8f,0.125f,3f)]
    public Color colourLow;
    [ColorUsageAttribute(true,true,0f,8f,0.125f,3f)]
    public Color colorHigh;
    public float startFresnelPower = 1f;
    public float endFresnelPower = 5f;
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Awake()
    {
        base.Awake();
        mat = renderer.material;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        List<ConduitVisual> visuals = new List<ConduitVisual>(connectedConduits.Count);

        foreach (var conduit in connectedConduits)
            visuals.Add(conduit);

        foreach (var id in visuals)
        {
            RemoveConnectedConduit(id);
            GameFrontendManager.Instance.DeleteConnection(id.backendID);
        }
            
        
        connectedConduits.Clear();
       
    }
    public override void AddConnectedConduit(ConduitVisual conduitVisual, Direction dir)
    {
        connectedConduits.Add(conduitVisual);
    }
 
    public void UpdateHealthBar(float hp)
    {
        Color currentColor = Color.Lerp(colourLow, colorHigh, hp);
        mat.SetColor("_FresnelColor", currentColor);
        float fresnelPower = Mathf.Lerp(startFresnelPower, endFresnelPower, hp);
        mat.SetFloat("_FresnelPower", fresnelPower);
    }
}
