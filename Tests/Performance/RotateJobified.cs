using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Gilzoide.EasyTransformJob.Tests.Performance
{
    public struct RotateJob : IUpdateJob
    {
        public float Speed;
        public float Delta;
        public float Max;
        public FixedBytes4094 Bytes;

        public void Process(TransformAccess transform)
        {
            var rotation = Vector3.one * (Speed * UpdateJobTime.DeltaTime);
            transform.localRotation *= Quaternion.Euler(rotation.x, rotation.y, rotation.z);
            Speed = Mathf.Clamp(Speed + Delta, 0, Max);
        }
    }

    public class RotateJobified : AJobBehaviour<RotateJob>
    {
        public RotateJob _velocidade = new RotateJob
        {
            Speed = 0,
            Delta = 0.1f,
            Max = 100,
        };

        public override RotateJob InitialJobData => _velocidade;

        void PrintData()
        {
            Debug.Log($"VELOCIDADE: {JobData.Speed}", this);
        }

        // public RotateJob Data;
        // {
        //     get => _velocidade;
        //     set => _velocidade = value;
        // }
    }
}

