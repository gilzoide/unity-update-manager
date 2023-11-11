using Gilzoide.UpdateManager.Jobs.Internal;

namespace Gilzoide.UpdateManager.Jobs
{
    /// <summary>
    /// Update job structs must implement this interface to be executed by <see cref="UpdateJobManager{}"/>.
    /// </summary>
    public interface IUpdateJob
    {
        void Execute();
    }

    /// <summary>
    /// A Burst-enabled version of <see cref="IUpdateJob"/>.
    /// Implement this if you want to have your job compiled by Burst.
    /// </summary>
    /// <typeparam name="TBurstJob">
    ///   A concrete version of <see cref="BurstUpdateJob{}"/> using the job type itself as type parameter.
    /// </typeparam>
    public interface IBurstUpdateJob<TBurstJob> : IUpdateJob
        where TBurstJob : IInternalBurstUpdateJob
    {
    }
}
