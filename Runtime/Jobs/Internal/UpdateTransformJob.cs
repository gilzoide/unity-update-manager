using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager.Jobs.Internal
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
}
