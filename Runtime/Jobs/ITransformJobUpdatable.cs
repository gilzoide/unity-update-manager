using Gilzoide.UpdateManager.Jobs.Internal;

namespace Gilzoide.UpdateManager.Jobs
{
    /// <summary>
    /// Implement this interface and register object with <see cref="UpdateTransformJobManager{}.Register"/> to get job scheduled every frame.
    /// </summary>
    public interface ITransformJobUpdatable<TData> : IInitialTransformJobDataProvider<TData>
        where TData : struct, IUpdateTransformJob
    {
    }

    /// <summary>
    /// Alias for <see cref="ITransformJobUpdatable{}"/>.
    /// Pass <c>BurstUpdateTransformJob&lt;<typeparamref name="TData"/>&gt;</c> as <typeparamref name="TJob"/> to Burst compile your job.
    /// </summary>
    public interface ITransformJobUpdatable<TData, TJob> : ITransformJobUpdatable<TData>
        where TData : struct, IUpdateTransformJob
        where TJob : struct, IInternalUpdateTransformJob<TData>
    {
    }

    public static class ITransformJobUpdatableExtensions
    {
        /// <summary>
        /// Shortcut for <c>UpdateTransformJobManager&lt;TData&gt;.Instance.Register(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateTransformJobManager{}.Register"/>
        public static void RegisterInManager<TData>(this ITransformJobUpdatable<TData> updatable, bool syncEveryFrame = false)
            where TData : struct, IUpdateTransformJob
        {
            UpdateTransformJobManager<TData>.Instance.Register(updatable, syncEveryFrame);
        }

        /// <summary>
        /// Shortcut for <c>UpdateTransformJobManager&lt;TData&gt;.Instance.Unregister(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateTransformJobManager{}.Unregister"/>
        public static void UnregisterInManager<TData>(this ITransformJobUpdatable<TData> updatable)
            where TData : struct, IUpdateTransformJob
        {
            UpdateTransformJobManager<TData>.Instance.Unregister(updatable);
        }

        /// <summary>
        /// Shortcut for <c>UpdateTransformJobManager&lt;TData&gt;.Instance.UnregisterSynchronization(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateTransformJobManager{}.UnregisterSynchronization"/>
        public static void UnregisterSynchronizationInManager<TData>(this ITransformJobUpdatable<TData> updatable)
            where TData : struct, IUpdateTransformJob
        {
            UpdateTransformJobManager<TData>.Instance.UnregisterSynchronization(updatable);
        }

        /// <summary>
        /// Shortcut for <c>UpdateTransformJobManager&lt;TData&gt;.Instance.IsRegistered(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateTransformJobManager{}.IsRegistered"/>
        public static bool IsRegisteredInManager<TData>(this ITransformJobUpdatable<TData> updatable)
            where TData : struct, IUpdateTransformJob
        {
            return UpdateTransformJobManager<TData>.Instance.IsRegistered(updatable);
        }

        /// <summary>
        /// Shortcut for <c>UpdateTransformJobManager&lt;TData&gt;.Instance.GetData(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateTransformJobManager{}.GetData"/>
        public static TData GetJobData<TData>(this ITransformJobUpdatable<TData> updatable)
            where TData : struct, IUpdateTransformJob
        {
            return UpdateTransformJobManager<TData>.Instance.GetData(updatable);
        }

        /// <summary>
        /// Shortcut for <c>UpdateTransformJobManager&lt;TData&gt;.Instance.SynchronizeJobDataOnce(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateJobManager{}.SynchronizeJobDataOnce"/>
        public static void SynchronizeJobDataOnce<TData>(this ITransformJobUpdatable<TData> updatable)
            where TData : struct, IUpdateTransformJob
        {
            UpdateTransformJobManager<TData>.Instance.SynchronizeJobDataOnce(updatable);
        }
    }
}
