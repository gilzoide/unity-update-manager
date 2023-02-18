using Gilzoide.UpdateManager.Jobs.Internal;

namespace Gilzoide.UpdateManager.Jobs
{
    /// <summary>
    /// Implement this interface and register object with <see cref="UpdateJobManager{,}.Register"/> to get job scheduled every frame.
    /// </summary>
    /// <remarks>
    /// To use Burst-compiled jobs, pass <see cref="BurstUpdateJob{}"/> as <typeparamref name="TJob"/>.
    /// </remarks>
    public interface IJobUpdatable<TData, TJob> : IInitialJobDataProvider<TData>
        where TData : struct, IUpdateJob
        where TJob : struct, IInternalUpdateJob<TData>
    {
    }

    /// <summary>
    /// Alias for <see cref="IJobUpdatable{,}"/> that defaults to using jobs that are not Burst compilable.
    /// </summary>
    public interface IJobUpdatable<TData> : IJobUpdatable<TData, UpdateJob<TData>>
        where TData : struct, IUpdateJob
    {
    }

    public static class IJobUpdatableExtensions
    {
        /// <summary>
        /// Shortcut for <c>UpdateJobManager&lt;TData, TJob&gt;.Instance.Register(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateJobManager{,}.Register"/>
        public static void RegisterInManager<TData, TJob>(this IJobUpdatable<TData, TJob> updatable)
            where TData : struct, IUpdateJob
            where TJob : struct, IInternalUpdateJob<TData>
        {
            UpdateJobManager<TData, TJob>.Instance.Register(updatable);
        }

        /// <summary>
        /// Shortcut for <c>UpdateJobManager&lt;TData, TJob&gt;.Instance.Unregister(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateJobManager{,}.Unregister"/>
        public static void UnregisterInManager<TData, TJob>(this IJobUpdatable<TData, TJob> updatable)
            where TData : struct, IUpdateJob
            where TJob : struct, IInternalUpdateJob<TData>
        {
            UpdateJobManager<TData, TJob>.Instance.Unregister(updatable);
        }

        /// <summary>
        /// Shortcut for <c>UpdateJobManager&lt;TData, TJob&gt;.Instance.GetData(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateJobManager{,}.GetData"/>
        public static TData GetJobData<TData, TJob>(this IJobUpdatable<TData, TJob> updatable)
            where TData : struct, IUpdateJob
            where TJob : struct, IInternalUpdateJob<TData>
        {
            return UpdateJobManager<TData, TJob>.Instance.GetData(updatable);
        }
    }
}
