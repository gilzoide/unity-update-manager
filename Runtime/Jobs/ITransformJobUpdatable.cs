using Gilzoide.UpdateManager.Jobs.Internal;

namespace Gilzoide.UpdateManager.Jobs
{
    public interface ITransformJobUpdatable<TData> : IInitialTransformJobDataProvider<TData>
        where TData : struct, IUpdateTransformJob
    {
    }

    public static class ITransformJobUpdatableExtensions
    {
        public static void RegisterInManager<TData>(this ITransformJobUpdatable<TData> updatable)
            where TData : struct, IUpdateTransformJob
        {
            UpdateTransformJobManager<TData>.Instance.Register(updatable);
        }

        public static void UnregisterInManager<TData>(this ITransformJobUpdatable<TData> updatable)
            where TData : struct, IUpdateTransformJob
        {
            UpdateTransformJobManager<TData>.Instance.Unregister(updatable);
        }

        public static TData GetJobData<TData>(this ITransformJobUpdatable<TData> updatable)
            where TData : struct, IUpdateTransformJob
        {
            return UpdateTransformJobManager<TData>.Instance.GetData(updatable);
        }
    }
}
