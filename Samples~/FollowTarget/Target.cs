using Gilzoide.UpdateManager.Jobs;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager.Sample.FollowTarget
{
#if HAVE_BURST
    [Unity.Burst.BurstCompile]
#endif
    [ReadOnlyTransformAccess]
    public struct TargetJob : IUpdateTransformJob
    {
        [WriteOnly] public NativeReference<Vector3> PositionReference;

        public void Execute(TransformAccess transform)
        {
            PositionReference.Value = transform.position;
        }
    }

    public class Target : AJobBehaviour<TargetJob, BurstUpdateTransformJob<TargetJob>>
    {
        public NativeReference<Vector3> PositionReference => positionReference;
        private NativeReference<Vector3> positionReference;
        

        public override TargetJob InitialJobData => new TargetJob
        {
            PositionReference = positionReference,
        };

        void Start()
        {
            positionReference = new NativeReference<Vector3>(transform.position, Allocator.Persistent);
        }

        void OnDestroy()
        {
            positionReference.Dispose();
        }
    }
}
