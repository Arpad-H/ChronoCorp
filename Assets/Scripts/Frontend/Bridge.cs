using UnityEngine;
using UnityEngine.UI;

public class Bridge : MonoBehaviour
{
    public Material validBridgeMaterial;
    public Material invalidBridgeMaterial;
    public MeshRenderer bridgeMeshRenderer;
   
    
    public void SetValidMaterial(bool isValid)
    {
        if (isValid)
        {
            bridgeMeshRenderer.material = validBridgeMaterial;
        }
        else
        {
            bridgeMeshRenderer.material = invalidBridgeMaterial;
        }
    }
    
}
