using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public struct UpdateTransformJob<TData> : IJobParallelForTransform
        where TData : struct, IUpdateTransformJob
    {
        public UnsafeNativeList<TData> Data;

        public unsafe void Execute(int index, TransformAccess transform)
        {
            Data.ItemRefAt(index).Execute(transform);
        }
    }
}
