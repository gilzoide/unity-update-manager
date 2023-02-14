using Gilzoide.UpdateManager.Jobs.Internal;

namespace Gilzoide.UpdateManager.Jobs
{
    public interface IJobUpdatable<TData> : IInitialJobDataProvider<TData>
        where TData : struct, IUpdateJob
    {
    }

    public static class IJobUpdatableExtensions
    {
        public static void RegisterInManager<TData>(this IJobUpdatable<TData> updatable)
            where TData : struct, IUpdateJob
        {
            UpdateJobManager<TData>.Instance.Register(updatable);
        }

        public static void UnregisterInManager<TData>(this IJobUpdatable<TData> updatable)
            where TData : struct, IUpdateJob
        {
            UpdateJobManager<TData>.Instance.Unregister(updatable);
        }

        public static TData GetJobData<TData>(this IJobUpdatable<TData> updatable)
            where TData : struct, IUpdateJob
        {
            return UpdateJobManager<TData>.Instance.GetData(updatable);
        }
    }
}
