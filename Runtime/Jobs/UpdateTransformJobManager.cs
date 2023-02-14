using Gilzoide.UpdateManager.Jobs.Internal;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager.Jobs
{
    public class UpdateTransformJobManager<TData> : AUpdateJobManager<TData, ITransformJobUpdatable<TData>, UpdateTransformJobData<TData, ITransformJobUpdatable<TData>>>
        where TData : struct, IUpdateTransformJob
    {
        public static int JobBatchSize = UpdateJobOptions.GetBatchSize<TData>();
        public static bool ReadOnlyTransforms = UpdateJobOptions.GetReadOnlyTransforms<TData>();

        public static UpdateTransformJobManager<TData> Instance => _instance != null ? _instance : (_instance = new UpdateTransformJobManager<TData>());
        private static UpdateTransformJobManager<TData> _instance;

        static UpdateTransformJobManager()
        {
            Application.quitting += () => _instance?.Dispose();
        }

        protected override JobHandle ScheduleJob()
        {
            var job = new UpdateTransformJob<TData>
            {
                Data = _jobData.Data,
            };
            if (ReadOnlyTransforms)
            {
                return job.ScheduleReadOnly(_jobData.Transforms, JobBatchSize);
            }
            else
            {
                return job.Schedule(_jobData.Transforms);
            };
        }

        protected struct UpdateTransformJob : IJobParallelForTransform
        {
            public UnsafeNativeList<TData> Data;

            public unsafe void Execute(int index, TransformAccess transform)
            {
                Data.ItemRefAt(index).Execute(transform);
            }
        }
    }
}
