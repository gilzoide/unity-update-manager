using Gilzoide.UpdateManager.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager.Tests.Performance
{
    public struct RotateJob : IUpdateTransformJob
    {
        public float Speed;
        public float Delta;
        public float Max;

        public void Execute(TransformAccess transform)
        {
            var rotation = Vector3.one * (Speed * UpdateJobTime.deltaTime);
            transform.localRotation *= Quaternion.Euler(rotation.x, rotation.y, rotation.z);
            Speed = Mathf.Clamp(Speed + Delta, 0, Max);
        }
    }

    public class RotateJobified : AJobBehaviour<RotateJob>
    {
        public override RotateJob InitialJobData => new RotateJob
        {
            Speed = 0,
            Delta = 0.1f,
            Max = 100,
        };
    }
}

