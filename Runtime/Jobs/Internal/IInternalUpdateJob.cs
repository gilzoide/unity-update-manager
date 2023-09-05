using Unity.Jobs;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public interface IInternalUpdateJob<TData> : IJobParallelFor
        where TData : struct
    {
        UnsafeNativeList<TData> Data { set; }
    }
}
