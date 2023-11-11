using System;
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
    /// </summary>
    /// <remarks>
    ///   Deprecated: use <see cref="IJobUpdatable{}"/> instead and implement
    ///   <see cref="IBurstUpdateJob{}"/> in job definition to compile jobs with Burst.
    /// </remarks>
    [Obsolete("Use IJobUpdatable<> and implement IBurstUpdateJob<> in job definition instead.")]
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
        /// Shortcut for <c>UpdateJobManager&lt;TData&gt;.Instance.Register(<paramref name="updatable"/>, <paramref name="syncEveryFrame"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateJobManager{}.Register"/>
        public static void RegisterInManager<TData>(this IJobUpdatable<TData> updatable, bool syncEveryFrame)
            where TData : struct, IUpdateJob
        {
            UpdateJobManager<TData>.Instance.Register(updatable, syncEveryFrame);
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
        /// Shortcut for <c>UpdateJobManager&lt;TData&gt;.Instance.UnregisterSynchronization(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateJobManager{}.UnregisterSynchronization"/>
        public static void UnregisterSynchronizationInManager<TData>(this IJobUpdatable<TData> updatable)
            where TData : struct, IUpdateJob
        {
            UpdateJobManager<TData>.Instance.UnregisterSynchronization(updatable);
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

        /// <summary>
        /// Shortcut for <c>UpdateJobManager&lt;TData&gt;.Instance.SynchronizeJobDataOnce(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateJobManager{}.SynchronizeJobDataOnce"/>
        public static void SynchronizeJobDataOnce<TData>(this IJobUpdatable<TData> updatable)
            where TData : struct, IUpdateJob
        {
            UpdateJobManager<TData>.Instance.SynchronizeJobDataOnce(updatable);
        }
    }
}
