using Unity.Collections;
using UnityEngine.Jobs;

namespace Gilzoide.EasyTransformJob
{
    public struct TransformJob<TData> : IJobParallelForTransform
        where TData : struct, ITransformJob
    {
        [ReadOnly] public NativeArray<TData> Data;
        public float DeltaTime;

        public void Execute(int index, TransformAccess transform)
        {
            Data[index].Process(transform, DeltaTime);
        }
    }
}
