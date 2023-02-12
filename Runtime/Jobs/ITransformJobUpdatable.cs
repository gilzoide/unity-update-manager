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
        public static TData GetJobData<TData>(this ITransformJobUpdatable<TData> updatable)
            where TData : struct, IUpdateTransformJob
        {
            return UpdateTransformJobManager<TData>.Instance.GetData(updatable);
        }
    }
}
