using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;

namespace Gilzoide.EasyTransformJob
{
    public struct UpdateJob<TData> : IJobParallelForTransform
        where TData : struct, IUpdateJob
    {
        public NativeArray<TData> Data;

        public void Execute(int index, TransformAccess transform)
        {
            var data = Data[index];
            data.Process(transform);
            Data[index] = data;
        }
    }
}
