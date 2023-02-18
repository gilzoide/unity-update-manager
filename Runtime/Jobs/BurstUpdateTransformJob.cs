#if HAVE_BURST
using Gilzoide.UpdateManager.Jobs.Internal;
using Unity.Burst;
using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager.Jobs
{
    /// <summary>
    /// Burst-enabled and <see cref="TransformAccess"/>-enabled update job struct. 
    /// </summary>
    [BurstCompile]
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
#endif