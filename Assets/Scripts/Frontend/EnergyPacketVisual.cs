using UnityEngine;

public class EnergyPacketVisual : MonoBehaviour
{
    private readonly float speed = 0.2f;
    private Conduit conduit;
    private float progress;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
    }


    private void LateUpdate()
    {
        //lerp between conduit.nodeA and conduit.nodeB based on progress
        if (conduit != null)
        {
            if (progress >= 1f) EnergyPacketVisualizer.Instance.ReleaseItem(this);
            var startPos = conduit.nodeA.GetAttachPosition();
            var endPos = conduit.nodeB.GetAttachPosition();
            transform.position = Vector3.Lerp(startPos, endPos, progress);
        }

        progress += Time.deltaTime * speed; //speed factor
    }

    public void SetConduit(Conduit conduit)
    {
        this.conduit = conduit;
        progress = 0f; //reset progress
    }
}