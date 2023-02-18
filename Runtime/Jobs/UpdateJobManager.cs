using Gilzoide.UpdateManager.Jobs.Internal;
using Unity.Jobs;
using UnityEngine;

namespace Gilzoide.UpdateManager.Jobs
{
    public class UpdateJobManager<TData, TJob> : AUpdateJobManager<TData, IJobUpdatable<TData, TJob>, UpdateJobData<TData, IJobUpdatable<TData, TJob>>>
        where TData : struct, IUpdateJob
        where TJob : struct, IInternalUpdateJob<TData>
    {
        public static int JobBatchSize = UpdateJobOptions.GetBatchSize<TData>();

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

    public class UpdateJobManager<TData> : UpdateJobManager<TData, UpdateJob<TData>>
        where TData : struct, IUpdateJob
    {
    }
}
