using Gilzoide.UpdateManager.Jobs.Internal;
using Unity.Jobs;
using UnityEngine;

namespace Gilzoide.UpdateManager.Jobs
{
    public class UpdateJobManager<TData> : AUpdateJobManager<TData, IJobUpdatable<TData>, UpdateJobData<TData, IJobUpdatable<TData>>>
        where TData : struct, IUpdateJob
    {
        public static int JobBatchSize = UpdateJobOptions.GetBatchSize<TData>();

        public static UpdateJobManager<TData> Instance => _instance != null ? _instance : (_instance = new UpdateJobManager<TData>());
        private static UpdateJobManager<TData> _instance;

        static UpdateJobManager()
        {
            Application.quitting += () => _instance?.Dispose();
        }

        protected override JobHandle ScheduleJob(JobHandle dependsOn)
        {
            return new UpdateJob
            {
                Data = _jobData.Data,
            }.Schedule(_jobData.Length, JobBatchSize, dependsOn);
        }

        protected struct UpdateJob : IJobParallelFor
        {
            public UnsafeNativeList<TData> Data;

            public unsafe void Execute(int index)
            {
                Data.ItemRefAt(index).Execute();
            }
        }
    }
}
