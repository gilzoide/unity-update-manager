using Gilzoide.UpdateManager.Jobs.Internal;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager.Jobs
{
    /// <summary>
    /// Singleton class that schedules <see cref="TransformAccess"/>-enabled jobs from registered <see cref="ITransformJobUpdatable{}>"/> objects every frame using Unity's Job System.
    /// </summary>
    /// <remarks>
    /// Any C# object can be registered for updates, including MonoBehaviours, pure C# classes and structs, as long as they implement <see cref="ITransformJobUpdatable{}"/>.
    /// <br/>
    /// This class runs jobs in parallel, so don't rely on jobs being executed in any order.
    /// </remarks>
    /// <seealso cref="IJobParallelForTransformExtensions.Schedule"/>
    /// <seealso cref="IJobParallelForTransformExtensions.ScheduleReadOnly"/>
    public class UpdateTransformJobManager<TData> : AUpdateJobManager<TData, ITransformJobUpdatable<TData>, UpdateTransformJobData<TData, ITransformJobUpdatable<TData>>>
        where TData : struct, IUpdateTransformJob
    {
        public static readonly int JobBatchSize = UpdateJobOptions.GetBatchSize<TData>();
        public static readonly bool ReadOnlyTransformAccess = UpdateJobOptions.GetReadOnlyTransformAccess<TData>();

        /// <summary>Get or create the singleton instance</summary>
        public static UpdateTransformJobManager<TData> Instance => _instance != null ? _instance : (_instance = new UpdateTransformJobManager<TData>());
        private static UpdateTransformJobManager<TData> _instance;

        protected override JobHandle ScheduleJob(JobHandle dependsOn)
        {
#if HAVE_BURST
            if (IsJobBurstCompiled)
            {
                var job = new BurstUpdateTransformJob<TData>
                {
                    Data = _jobData.Data,
                };
                return ReadOnlyTransformAccess
                    ? job.ScheduleReadOnly(_jobData.Transforms, JobBatchSize, dependsOn)
                    : job.Schedule(_jobData.Transforms, dependsOn);
            }
            else
#endif
            {
                var job = new UpdateTransformJob<TData>
                {
                    Data = _jobData.Data,
                };
                return ReadOnlyTransformAccess
                    ? job.ScheduleReadOnly(_jobData.Transforms, JobBatchSize, dependsOn)
                    : job.Schedule(_jobData.Transforms, dependsOn);
            }
        }
    }
}
