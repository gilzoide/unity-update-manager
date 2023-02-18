#if HAVE_BURST
using Gilzoide.UpdateManager.Jobs.Internal;
using Unity.Burst;

namespace Gilzoide.UpdateManager.Jobs
{
    [BurstCompile]
    public struct BurstUpdateJob<TData> : IInternalUpdateJob<TData>
        where TData : struct, IUpdateJob
    {
        public UnsafeNativeList<TData> Data { get; set; }

        public unsafe void Execute(int index)
        {
            Data.ItemRefAt(index).Execute();
        }
    }
}
#endif