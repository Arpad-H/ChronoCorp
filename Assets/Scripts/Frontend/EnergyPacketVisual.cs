using UnityEngine;

public class EnergyPacketVisual : MonoBehaviour
{
    Conduit conduit;
    float progress = 0f;
    float speed = 0.2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public void SetConduit(Conduit conduit)
    {
        this.conduit = conduit;
        progress = 0f; //reset progress
    }

   
    void LateUpdate()
    {
        //lerp between conduit.nodeA and conduit.nodeB based on progress
        if (conduit != null)
        {
            if (progress >= 1f) EnergyPacketVisualizer.Instance.ReleaseItem(this);
            Vector3 startPos = conduit.nodeA.GetAttachPosition();
            Vector3 endPos = conduit.nodeB.GetAttachPosition();
            transform.position = Vector3.Lerp(startPos, endPos, progress);
        }

        progress += Time.deltaTime * speed; //speed factor
    }
}