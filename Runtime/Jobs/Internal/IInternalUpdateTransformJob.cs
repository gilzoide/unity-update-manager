using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public interface IInternalUpdateTransformJob<TData> : IJobParallelForTransform
        where TData : struct
    {
        UnsafeNativeList<TData> Data { set; }
    }

    public interface IInternalBurstUpdateTransformJob
    {
    }
}
