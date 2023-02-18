using Gilzoide.UpdateManager.Jobs.Internal;
using Unity.Jobs;
using UnityEngine;

namespace Gilzoide.UpdateManager.Jobs
{
    /// <summary>
    /// Singleton class that schedules jobs from registered <see cref="IJobUpdatable{}>"/> objects every frame using Unity's Job System.
    /// </summary>
    /// <remarks>
    /// Any C# object can be registered for updates, including MonoBehaviours, pure C# classes and structs, as long as they implement <see cref="IJobUpdatable{}"/>.
    /// <br/>
    /// This class runs jobs in parallel, so don't rely on jobs being executed in any order.
    /// <br/>
    /// To use Burst-compiled jobs, pass <see cref="BurstUpdateJob{}"/> as <typeparamref name="TJob"/>.
    /// </remarks>
    public class UpdateJobManager<TData, TJob> : AUpdateJobManager<TData, IJobUpdatable<TData, TJob>, UpdateJobData<TData, IJobUpdatable<TData, TJob>>>
        where TData : struct, IUpdateJob
        where TJob : struct, IInternalUpdateJob<TData>
    {
        public static int JobBatchSize = UpdateJobOptions.GetBatchSize<TData>();

        /// <summary>Get or create the singleton instance</summary>
        public static UpdateJobManager<TData, TJob> Instance => _instance != null ? _instance : (_instance = new UpdateJobManager<TData, TJob>());
        private static UpdateJobManager<TData, TJob> _instance;

        static UpdateJobManager()
        {
            Application.quitting += () => _instance?.Dispose();
        }

        protected unsafe override JobHandle ScheduleJob(JobHandle dependsOn)
        {
            return new TJob
            {
                Data = _jobData.Data,
            }.Schedule(_jobData.Length, JobBatchSize, dependsOn);
        }
    }

    /// <summary>
    /// Alias for <see cref="UpdateJobManager{,}"/> that defaults to using jobs that are not Burst compilable.
    /// </summary>
    public class UpdateJobManager<TData> : UpdateJobManager<TData, UpdateJob<TData>>
        where TData : struct, IUpdateJob
    {
    }
}
