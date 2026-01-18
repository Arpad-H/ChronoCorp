using UnityEngine;

namespace Util
{
    public class ConstantRotation : MonoBehaviour
    {
        [Header("Rotation Settings")] [Tooltip("Degrees per second")]
        public float rotationSpeed = 50f;

        void Update()
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }
}