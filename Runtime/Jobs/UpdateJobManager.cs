using System;
using Gilzoide.UpdateManager.Jobs.Internal;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
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

        protected unsafe override JobHandle ScheduleJob(JobHandle dependsOn)
        {
            var scheduleParams = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref _jobData.DataRef), GetReflectionData(), dependsOn, ScheduleMode.Parallel);
            return JobsUtility.ScheduleParallelFor(ref scheduleParams, _jobData.Length, JobBatchSize);
        }

        #region Job Producer

        public delegate void ExecuteJobFunction(ref UnsafeNativeList<TData> jobData, IntPtr jobData2, IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex);
        public static unsafe void Execute(ref UnsafeNativeList<TData> jobData, IntPtr jobData2, IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex)
        {
            while (JobsUtility.GetWorkStealingRange(ref ranges, jobIndex, out int begin, out int end))
            {
                JobsUtility.PatchBufferMinMaxRanges(bufferRangePatchData, UnsafeUtility.AddressOf(ref jobData), begin, end - begin);

                int endThatCompilerCanSeeWillNeverChange = end;
                for (int i = begin; i < endThatCompilerCanSeeWillNeverChange; ++i)
                {
                    jobData.ItemRefAt(i).Execute();
                }
            }
        }

        private static IntPtr _reflectionData;
        private static IntPtr GetReflectionData()
        {
            if (_reflectionData == IntPtr.Zero)
            {
                _reflectionData = JobsUtility.CreateJobReflectionData(typeof(UnsafeNativeList<TData>), (ExecuteJobFunction) Execute);
            }
            return _reflectionData;
        }

        #endregion
    }
}
