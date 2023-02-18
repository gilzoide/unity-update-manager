using Gilzoide.UpdateManager.Jobs.Internal;
using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager
{
    public interface IInternalUpdateTransformJob<TData> : IJobParallelForTransform
        where TData : struct
    {
        UnsafeNativeList<TData> Data { set; }
    }
}
