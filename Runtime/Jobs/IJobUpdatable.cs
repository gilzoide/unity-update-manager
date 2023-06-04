using Gilzoide.UpdateManager.Jobs.Internal;

namespace Gilzoide.UpdateManager.Jobs
{
    /// <summary>
    /// Implement this interface and register object with <see cref="UpdateJobManager{}.Register"/> to get job scheduled every frame.
    /// </summary>
    public interface IJobUpdatable<TData> : IInitialJobDataProvider<TData>
        where TData : struct, IUpdateJob
    {
    }

    /// <summary>
    /// Alias for <see cref="IJobUpdatable{}"/>.
    /// Pass <c>UpdateJob&lt;<typeparamref name="TData"/>&gt;</c> as <typeparamref name="TJob"/> to Burst compile your job.
    /// </summary>
    public interface IJobUpdatable<TData, TJob> : IJobUpdatable<TData>
        where TData : struct, IUpdateJob
        where TJob : struct, IInternalUpdateJob<TData>
    {
    }

    public static class IJobUpdatableExtensions
    {
        /// <summary>
        /// Shortcut for <c>UpdateJobManager&lt;TData&gt;.Instance.Register(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateJobManager{}.Register"/>
        public static void RegisterInManager<TData>(this IJobUpdatable<TData> updatable)
            where TData : struct, IUpdateJob
        {
            UpdateJobManager<TData>.Instance.Register(updatable);
        }

        /// <summary>
        /// Shortcut for <c>UpdateJobManager&lt;TData&gt;.Instance.Unregister(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateJobManager{}.Unregister"/>
        public static void UnregisterInManager<TData>(this IJobUpdatable<TData> updatable)
            where TData : struct, IUpdateJob
        {
            UpdateJobManager<TData>.Instance.Unregister(updatable);
        }

        /// <summary>
        /// Shortcut for <c>UpdateJobManager&lt;TData&gt;.Instance.IsRegistered(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateJobManager{}.IsRegistered"/>
        public static bool IsRegisteredInManager<TData>(this IJobUpdatable<TData> updatable)
            where TData : struct, IUpdateJob
        {
            return UpdateJobManager<TData>.Instance.IsRegistered(updatable);
        }

        /// <summary>
        /// Shortcut for <c>UpdateJobManager&lt;TData&gt;.Instance.GetData(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateJobManager{}.GetData"/>
        public static TData GetJobData<TData>(this IJobUpdatable<TData> updatable)
            where TData : struct, IUpdateJob
        {
            return UpdateJobManager<TData>.Instance.GetData(updatable);
        }
    }
}
