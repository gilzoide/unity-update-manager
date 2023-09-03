namespace Gilzoide.UpdateManager.Jobs
{
    /// <summary>
    /// Implement this in your <see cref="IJobUpdatable{}"/> or <see cref="ITransformJobUpdatable{}"/>
    /// classes to be able to read/write job data.
    /// </summary>
    /// <remarks>
    /// Job data is always synchronized while no jobs are running, so that it can be safely modified.
    /// <br/>
    /// Provider registration/unregistration during data synchronization is supported and
    /// will be effective in the same frame.
    /// </remarks>
    /// <seealso cref="AUpdateJobManager{,,}.Register"/>
    /// <seealso cref="AUpdateJobManager{,,}.SynchronizeJobDataOnce"/>
    /// <seealso cref="AUpdateJobManager{,,}.UnregisterSynchronization"/>
    public interface IJobDataSynchronizer<T>
    {
        void SyncJobData(ref T jobData);
    }
}
