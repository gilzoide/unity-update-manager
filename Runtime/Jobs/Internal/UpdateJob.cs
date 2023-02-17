namespace Gilzoide.UpdateManager.Jobs.Internal
{
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
