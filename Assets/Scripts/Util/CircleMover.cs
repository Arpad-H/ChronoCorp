using UnityEngine;
//util to make something moves in a circle to test trails and such
public class CircleMover : MonoBehaviour
{
    public float radius = 5f;
    public float speed = 1f;

    private float angle;

    void Update()
    {
        angle += speed * Time.deltaTime;
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        transform.position = new Vector3(x, 0f, z);
    }
}