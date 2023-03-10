using System;

namespace Gilzoide.UpdateManager.Jobs
{
    [AttributeUsage(AttributeTargets.Struct), Obsolete("Use JobBatchSizeAttribute and ReadOnlyTransformAccessAttribute instead")]
    public class UpdateJobOptionsAttribute : Attribute
    {
        public int BatchSize { get; set; }
        public bool ReadOnlyTransforms { get; set; }
    }
}
