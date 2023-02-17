using Gilzoide.UpdateManager.Jobs.Internal;

namespace Gilzoide.UpdateManager.Jobs
{
    public interface ITransformJobUpdatable<TData, TJob> : IInitialTransformJobDataProvider<TData>
        where TData : struct, IUpdateTransformJob
        where TJob : struct, IInternalUpdateTransformJob<TData>
    {
    }

    public interface ITransformJobUpdatable<TData> : ITransformJobUpdatable<TData, UpdateTransformJob<TData>>
        where TData : struct, IUpdateTransformJob
    {
    }

    public static class ITransformJobUpdatableExtensions
    {
        public static void RegisterInManager<TData, TJob>(this ITransformJobUpdatable<TData, TJob> updatable)
            where TData : struct, IUpdateTransformJob
            where TJob : struct, IInternalUpdateTransformJob<TData>
        {
            UpdateTransformJobManager<TData, TJob>.Instance.Register(updatable);
        }

        public static void UnregisterInManager<TData, TJob>(this ITransformJobUpdatable<TData, TJob> updatable)
            where TData : struct, IUpdateTransformJob
            where TJob : struct, IInternalUpdateTransformJob<TData>
        {
            UpdateTransformJobManager<TData, TJob>.Instance.Unregister(updatable);
        }

        public static TData GetJobData<TData, TJob>(this ITransformJobUpdatable<TData, TJob> updatable)
            where TData : struct, IUpdateTransformJob
            where TJob : struct, IInternalUpdateTransformJob<TData>
        {
            return UpdateTransformJobManager<TData, TJob>.Instance.GetData(updatable);
        }
    }
}
