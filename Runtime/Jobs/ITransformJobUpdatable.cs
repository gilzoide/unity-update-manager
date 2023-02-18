using Gilzoide.UpdateManager.Jobs.Internal;

namespace Gilzoide.UpdateManager.Jobs
{
    /// <summary>
    /// Implement this interface and register object with <see cref="UpdateTransformJobManager{,}.Register"/> to get job scheduled every frame.
    /// </summary>
    /// <remarks>
    /// To use Burst-compiled jobs, pass <see cref="BurstUpdateTransformJob{}"/> as <typeparamref name="TJob"/>.
    /// </remarks>
    public interface ITransformJobUpdatable<TData, TJob> : IInitialTransformJobDataProvider<TData>
        where TData : struct, IUpdateTransformJob
        where TJob : struct, IInternalUpdateTransformJob<TData>
    {
    }

    /// <summary>
    /// Alias for <see cref="IJobUpdatable{,}"/> that defaults to using jobs that are not Burst compilable.
    /// </summary>
    public interface ITransformJobUpdatable<TData> : ITransformJobUpdatable<TData, UpdateTransformJob<TData>>
        where TData : struct, IUpdateTransformJob
    {
    }

    public static class ITransformJobUpdatableExtensions
    {
        /// <summary>
        /// Shortcut for <c>UpdateTransformJobManager&lt;TData, TJob&gt;.Instance.Register(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateTransformJobManager{,}.Register"/>
        public static void RegisterInManager<TData, TJob>(this ITransformJobUpdatable<TData, TJob> updatable)
            where TData : struct, IUpdateTransformJob
            where TJob : struct, IInternalUpdateTransformJob<TData>
        {
            UpdateTransformJobManager<TData, TJob>.Instance.Register(updatable);
        }

        /// <summary>
        /// Shortcut for <c>UpdateTransformJobManager&lt;TData, TJob&gt;.Instance.Unregister(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateTransformJobManager{,}.Unregister"/>
        public static void UnregisterInManager<TData, TJob>(this ITransformJobUpdatable<TData, TJob> updatable)
            where TData : struct, IUpdateTransformJob
            where TJob : struct, IInternalUpdateTransformJob<TData>
        {
            UpdateTransformJobManager<TData, TJob>.Instance.Unregister(updatable);
        }

        /// <summary>
        /// Shortcut for <c>UpdateTransformJobManager&lt;TData, TJob&gt;.Instance.GetData(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateTransformJobManager{,}.GetData"/>
        public static TData GetJobData<TData, TJob>(this ITransformJobUpdatable<TData, TJob> updatable)
            where TData : struct, IUpdateTransformJob
            where TJob : struct, IInternalUpdateTransformJob<TData>
        {
            return UpdateTransformJobManager<TData, TJob>.Instance.GetData(updatable);
        }
    }
}
