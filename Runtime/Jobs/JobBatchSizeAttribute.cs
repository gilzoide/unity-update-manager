using System;

namespace Gilzoide.UpdateManager.Jobs
{
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
