namespace Gilzoide.UpdateManager.Jobs
{
    public interface IJobUpdatable<TData>
        where TData : struct, IUpdateJob
    {
        TData InitialJobData { get; }
    }

    public static class IJobUpdatableExtensions
    {
        public static TData GetJobData<TData>(this IJobUpdatable<TData> updatable)
            where TData : struct, IUpdateJob
        {
            return UpdateJobManager<TData>.Instance.GetData(updatable);
        }
    }
}