using Unity.Collections;
using UnityEngine;

namespace Gilzoide.EasyTransformJob.Tests.Performance
{
    public class Rotate : MonoBehaviour
    {
        public float Speed = 0;
        public float Delta = 0.1f;
        public float Max = 100;
        public FixedBytes510 Bytes;

        void Update()
        {
            var rotation = Vector3.one * (Speed * Time.deltaTime);
            transform.localRotation *= Quaternion.Euler(rotation.x, rotation.y, rotation.z);
            Speed = Mathf.Clamp(Speed + Delta, 0, Max);
        }
    }
}
