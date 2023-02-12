using UnityEngine;

namespace Gilzoide.UpdateManager.Jobs
{
    public interface ITransformJobUpdatable<TData>
        where TData : struct, IUpdateTransformJob
    {
        Transform Transform { get; }
        TData InitialJobData { get; }
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
