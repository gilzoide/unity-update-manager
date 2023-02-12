namespace Gilzoide.UpdateManager.Jobs
{
    public interface IJobUpdatable<TData>
        where TData : struct, IUpdateJob
    {
        TData InitialJobData { get; }
    }
}
