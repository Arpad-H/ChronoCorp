using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class GlowHP : MonoBehaviour
{
    static readonly int HP_ID    = Shader.PropertyToID("_HP");
    static readonly int Color_ID = Shader.PropertyToID("_FillColor"); 
    static readonly int BGcolor_ID = Shader.PropertyToID("_EmptyColor"); 
    

    Renderer rend;
    MaterialPropertyBlock mpb;
    [SerializeField]
    int materialIndex = 0;
    void Awake()
    {
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    public void SetVisuals(float hp01, Color color)
    {
        // get current block (in case something else wrote to it)
        rend.GetPropertyBlock(mpb, materialIndex);

        mpb.SetFloat(HP_ID, hp01);
        mpb.SetColor(Color_ID, color);

        rend.SetPropertyBlock(mpb, materialIndex);
    }
    public void SetHP(float hp01)
    {
        // get current block (in case something else wrote to it)
        rend.GetPropertyBlock(mpb, materialIndex);

        mpb.SetFloat(HP_ID, hp01);

        rend.SetPropertyBlock(mpb, materialIndex);
    }
    public void SetBGColor(Color color)
    {
        rend.GetPropertyBlock(mpb, materialIndex);
        mpb.SetColor(BGcolor_ID, color);
        rend.SetPropertyBlock(mpb, materialIndex);
    }
}