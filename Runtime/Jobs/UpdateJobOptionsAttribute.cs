using System;

namespace Gilzoide.UpdateManager.Jobs
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class UpdateJobOptionsAttribute : Attribute
    {
        public int BatchSize { get; set; }
        public bool ReadOnlyTransforms { get; set; }
    }
}
