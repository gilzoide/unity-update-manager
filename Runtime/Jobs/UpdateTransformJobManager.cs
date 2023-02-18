using Gilzoide.UpdateManager.Jobs.Internal;
using Unity.Jobs;
using UnityEngine;
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
    /// <br/>
    /// To use Burst-compiled jobs, pass <see cref="BurstUpdateTransformJob{}"/> as <typeparamref name="TJob"/>.
    /// </remarks>
    public class UpdateTransformJobManager<TData, TJob> : AUpdateJobManager<TData, ITransformJobUpdatable<TData, TJob>, UpdateTransformJobData<TData, ITransformJobUpdatable<TData, TJob>>>
        where TData : struct, IUpdateTransformJob
        where TJob : struct, IInternalUpdateTransformJob<TData>
    {
        public static int JobBatchSize = UpdateJobOptions.GetBatchSize<TData>();
        public static bool ReadOnlyTransforms = UpdateJobOptions.GetReadOnlyTransforms<TData>();

        /// <summary>Get or create the singleton instance</summary>
        public static UpdateTransformJobManager<TData, TJob> Instance => _instance != null ? _instance : (_instance = new UpdateTransformJobManager<TData, TJob>());
        private static UpdateTransformJobManager<TData, TJob> _instance;

        static UpdateTransformJobManager()
        {
            Application.quitting += () => _instance?.Dispose();
        }

        protected override JobHandle ScheduleJob(JobHandle dependsOn)
        {
            var job = new TJob
            {
                Data = _jobData.Data,
            };
            return ReadOnlyTransforms
                ? job.ScheduleReadOnly(_jobData.Transforms, JobBatchSize, dependsOn)
                : job.Schedule(_jobData.Transforms, dependsOn);
        }
    }

    /// <summary>
    /// Alias for <see cref="UpdateTransformJobManager{,}"/> that defaults to using jobs that are not Burst compilable.
    /// </summary>
    public class UpdateTransformJobManager<TData> : UpdateTransformJobManager<TData, UpdateTransformJob<TData>>
        where TData : struct, IUpdateTransformJob
    {
    }
}
