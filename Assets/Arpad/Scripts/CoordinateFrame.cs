using UnityEngine;

public class CoordinatePlane : MonoBehaviour
{
    public Transform frameMesh;      // scaled mesh
    public Transform nodeContainer;  // unscaled coordinate space

    void Awake()
    {
        // Auto-find if not assigned
        if (!frameMesh) frameMesh = transform.Find("FrameMesh");
        if (!nodeContainer) nodeContainer = transform.Find("NodeContainer");
    }

    /// <summary>
    /// Converts plane coords (0→width, 0→height) into the nodeContainer local space
    /// </summary>
    public Vector3 PlaneToLocal(Vector2 planePos)
    {
        Vector3 scale = frameMesh.localScale;

        // X maps to local X; plane Y maps to local Z
        return new Vector3(
            planePos.x - scale.x / 2f,
            planePos.y - scale.z / 2f, 
            -0.01f
            
        );
    }

    /// <summary>
    /// Instantiates a node inside the nodeContainer at plane coordinates.
    /// </summary>
    public Transform PlaceNode(GameObject prefab, Vector2 planePos)
    {
        Vector3 localPos = PlaneToLocal(planePos);
        GameObject obj = Instantiate(prefab, nodeContainer);
        obj.transform.localPosition = localPos;
        return obj.transform;
    }
}