using UnityEngine;

namespace Gilzoide.UpdateManager.Tests.Performance
{
    public class RotateManaged : AUpdateManagerBehaviour
    {
        public float Speed = 0;
        public float Delta = 0.1f;
        public float Max = 100;

        public override void ManagedUpdate()
        {
            var rotation = Vector3.one * (Speed * Time.deltaTime);
            transform.localRotation *= Quaternion.Euler(rotation.x, rotation.y, rotation.z);
            Speed = Mathf.Clamp(Speed + Delta, 0, Max);
        }
    }
}
