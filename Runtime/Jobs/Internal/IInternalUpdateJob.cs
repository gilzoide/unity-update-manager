using Gilzoide.UpdateManager.Jobs.Internal;
using Unity.Jobs;

namespace Gilzoide.UpdateManager
{
    public interface IInternalUpdateJob<TData> : IJobParallelFor
        where TData : struct
    {
        UnsafeNativeList<TData> Data { set; }
    }
}
