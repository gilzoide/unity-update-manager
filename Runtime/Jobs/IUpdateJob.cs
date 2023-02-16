using Unity.Jobs.LowLevel.Unsafe;

namespace Gilzoide.UpdateManager.Jobs
{
    [JobProducerType(typeof(UpdateJobManager<>))]
    public interface IUpdateJob
    {
        void Execute();
    }
}
