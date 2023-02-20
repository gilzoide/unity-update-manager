using Gilzoide.UpdateManager.Jobs.Internal;

namespace Gilzoide.UpdateManager.Jobs
{
#if HAVE_BURST
    [Unity.Burst.BurstCompile]
#endif
    public struct UpdateJob<TData> : IInternalUpdateJob<TData>
        where TData : struct, IUpdateJob
    {
        public UnsafeNativeList<TData> Data { get; set; }

        public unsafe void Execute(int index)
        {
            Data.ItemRefAt(index).Execute();
        }
    }
}
