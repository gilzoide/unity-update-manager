using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public struct UpdateJob<TData> : IJobParallelFor
        where TData : struct, IUpdateJob
    {
        public NativeArray<TData> Data;

        public unsafe void Execute(int index)
        {
            void* dataPointer = NativeArrayUnsafeUtility.GetUnsafePtr(Data);
            UnsafeUtility.ArrayElementAsRef<TData>(dataPointer, index).Execute();
        }
    }
}
