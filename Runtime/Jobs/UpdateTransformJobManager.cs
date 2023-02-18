using Gilzoide.UpdateManager.Jobs.Internal;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager.Jobs
{
    public class UpdateTransformJobManager<TData, TJob> : AUpdateJobManager<TData, ITransformJobUpdatable<TData, TJob>, UpdateTransformJobData<TData, ITransformJobUpdatable<TData, TJob>>>
        where TData : struct, IUpdateTransformJob
        where TJob : struct, IInternalUpdateTransformJob<TData>
    {
        public static int JobBatchSize = UpdateJobOptions.GetBatchSize<TData>();
        public static bool ReadOnlyTransforms = UpdateJobOptions.GetReadOnlyTransforms<TData>();

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

    public class UpdateTransformJobManager<TData> : UpdateTransformJobManager<TData, UpdateTransformJob<TData>>
        where TData : struct, IUpdateTransformJob
    {
    }
}
