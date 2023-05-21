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
    /// </remarks>
    /// <seealso cref="Unity.Jobs.IJobParallelForExtensions.Schedule"/>
    public class UpdateJobManager<TData> : AUpdateJobManager<TData, IJobUpdatable<TData>, UpdateJobData<TData, IJobUpdatable<TData>>>
        where TData : struct, IUpdateJob
    {
        public static readonly int JobBatchSize = UpdateJobOptions.GetBatchSize<TData>();

        /// <summary>Get or create the singleton instance</summary>
        public static UpdateJobManager<TData> Instance => _instance != null ? _instance : (_instance = new UpdateJobManager<TData>());
        private static UpdateJobManager<TData> _instance;

        protected unsafe override JobHandle ScheduleJob(JobHandle dependsOn)
        {
            return new UpdateJob<TData>
            {
                Data = _jobData.Data,
            }.Schedule(_jobData.Length, JobBatchSize, dependsOn);
        }
    }
}
