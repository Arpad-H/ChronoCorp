using System;
using UnityEngine;

public class BulgeController : MonoBehaviour
{
    public Renderer renderer;
    private Material pipeMaterial;
    public float[] positions = { 0.1f, 0.3f, 0.9f }; // Your uneven positions

    private void Start()
    {
        pipeMaterial = renderer.material;
    }
    public void addBulge(float position)
    {
        Array.Resize(ref positions, positions.Length + 1);
        positions[positions.Length - 1] = position;
    }

    void LateUpdate()
    {
        // Pass the array to the shader
        pipeMaterial.SetFloatArray("_BulgePositions", positions);
        // Tell the shader how many elements in the array to actually loop through
        pipeMaterial.SetInt("_BulgeCount", positions.Length);
    }
}