using Gilzoide.UpdateManager.Jobs.Internal;

namespace Gilzoide.UpdateManager.Jobs
{
    public interface IJobUpdatable<TData, TJob> : IInitialJobDataProvider<TData>
        where TData : struct, IUpdateJob
        where TJob : struct, IInternalUpdateJob<TData>
    {
    }

    public interface IJobUpdatable<TData> : IJobUpdatable<TData, UpdateJob<TData>>
        where TData : struct, IUpdateJob
    {
    }

    public static class IJobUpdatableExtensions
    {
        public static void RegisterInManager<TData, TJob>(this IJobUpdatable<TData, TJob> updatable)
            where TData : struct, IUpdateJob
            where TJob : struct, IInternalUpdateJob<TData>
        {
            UpdateJobManager<TData, TJob>.Instance.Register(updatable);
        }

        public static void UnregisterInManager<TData, TJob>(this IJobUpdatable<TData, TJob> updatable)
            where TData : struct, IUpdateJob
            where TJob : struct, IInternalUpdateJob<TData>
        {
            UpdateJobManager<TData, TJob>.Instance.Unregister(updatable);
        }

        public static TData GetJobData<TData, TJob>(this IJobUpdatable<TData, TJob> updatable)
            where TData : struct, IUpdateJob
            where TJob : struct, IInternalUpdateJob<TData>
        {
            return UpdateJobManager<TData, TJob>.Instance.GetData(updatable);
        }
    }
}
