using UnityEngine.Jobs;
using Gilzoide.UpdateManager.Jobs.Internal;

namespace Gilzoide.UpdateManager.Jobs
{
    public struct UpdateTransformJob<TData> : IInternalUpdateTransformJob<TData>
        where TData : struct, IUpdateTransformJob
    {
        public UnsafeNativeList<TData> Data { get; set; }

        public unsafe void Execute(int index, TransformAccess transform)
        {
            Data.ItemRefAt(index).Execute(transform);
        }
    }

#if HAVE_BURST
    [Unity.Burst.BurstCompile]
#endif
    public struct BurstUpdateTransformJob<TData> : IInternalUpdateTransformJob<TData>
        where TData : struct, IUpdateTransformJob
    {
        public UnsafeNativeList<TData> Data { get; set; }

        public unsafe void Execute(int index, TransformAccess transform)
        {
            Data.ItemRefAt(index).Execute(transform);
        }
    }
}
