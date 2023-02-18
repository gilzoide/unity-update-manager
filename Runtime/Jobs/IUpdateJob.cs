namespace Gilzoide.UpdateManager.Jobs
{
    /// <summary>
    /// Update job structs must implement this interface to be executed by <see cref="UpdateJobManager{,}"/>.
    /// </summary>
    public interface IUpdateJob
    {
        void Execute();
    }
}
