using Unity.Jobs;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public struct UpdateJob<TData> : IJobParallelFor
        where TData : struct, IUpdateJob
    {
        public UnsafeNativeList<TData> Data;

        public unsafe void Execute(int index)
        {
            Data.ItemRefAt(index).Execute();
        }
    }
}
