using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public struct UpdateTransformJob<TData> : IJobParallelForTransform
        where TData : struct, IUpdateTransformJob
    {
        public NativeArray<TData> Data;

        public unsafe void Execute(int index, TransformAccess transform)
        {
            void* dataPointer = NativeArrayUnsafeUtility.GetUnsafePtr(Data);
            UnsafeUtility.ArrayElementAsRef<TData>(dataPointer, index).Execute(transform);
        }
    }
}
