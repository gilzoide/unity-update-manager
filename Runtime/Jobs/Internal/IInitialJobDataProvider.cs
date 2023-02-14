namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public interface IInitialJobDataProvider<TData>
    {
        TData InitialJobData { get; }
    }
}
