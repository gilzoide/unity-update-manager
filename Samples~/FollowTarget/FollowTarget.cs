using Gilzoide.UpdateManager.Jobs;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager.Sample.FollowTarget
{
    [DependsOn(typeof(TargetJob))]
    public struct FollowTargetJob : IUpdateTransformJob
    {
        [ReadOnly] public NativeReference<Vector3> TargetPositionReference;
        public float Speed;
        public float MinimumDistance;
        
        public void Execute(TransformAccess transform)
        {
            Vector3 targetPosition = TargetPositionReference.Value;
            // apply minimum distance
            Vector3 movementDirection = (targetPosition - transform.position).normalized;
            targetPosition -= movementDirection * MinimumDistance;
            // now set position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Speed * UpdateJobTime.deltaTime);
        }
    }

    public class FollowTarget : AJobBehaviour<FollowTargetJob, BurstUpdateTransformJob<FollowTargetJob>>,
        IJobDataSynchronizer<FollowTargetJob>
    {
        [SerializeField] private Target target;
        [SerializeField] private float speed;
        [SerializeField] private float minimumDistance;

        public override FollowTargetJob InitialJobData => new FollowTargetJob
        {
            TargetPositionReference = target.PositionReference,
            Speed = speed,
            MinimumDistance = minimumDistance,
        };

        public void SyncJobData(ref FollowTargetJob jobData)
        {
            jobData = InitialJobData;
        }
    }
}
