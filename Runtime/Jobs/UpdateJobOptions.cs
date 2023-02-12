using System.Reflection;

namespace Gilzoide.UpdateManager.Jobs
{
    public static class UpdateJobOptions
    {
        public const int DefaultBatchSize = 64;

        public static int GetBatchSize<TData>()
        {
            if (typeof(TData).GetCustomAttribute<UpdateJobOptionsAttribute>() is UpdateJobOptionsAttribute options
                && options.BatchSize > 0)
            {
                return options.BatchSize;
            }
            else
            {
                return DefaultBatchSize;
            }
        }

        public static bool GetReadOnlyTransforms<TData>()
        {
            return typeof(TData).GetCustomAttribute<UpdateJobOptionsAttribute>() is UpdateJobOptionsAttribute options
                && options.ReadOnlyTransforms;
        }
    }
}
