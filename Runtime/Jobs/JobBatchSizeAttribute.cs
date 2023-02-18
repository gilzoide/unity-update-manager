using System;

namespace Gilzoide.UpdateManager.Jobs
{
    /// <summary>
    /// Add this to a managed job struct type for setting its job batch size.
    /// </summary>
    /// <seealso cref="Unity.Jobs.IJobParallelForExtensions.Schedule"/>
    /// <seealso cref="UnityEngine.Jobs.IJobParallelForTransformExtensions.ScheduleReadOnly"/>
    [AttributeUsage(AttributeTargets.Struct)]
    public class JobBatchSizeAttribute : Attribute
    {
        public int BatchSize { get; private set; }

        public JobBatchSizeAttribute(int batchSize)
        {
            if (batchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be a positive number");
            }
            BatchSize = batchSize;
        }
    }
}
