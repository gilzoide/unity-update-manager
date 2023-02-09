using UnityEngine;

namespace Gilzoide.EasyTransformJob
{
    public interface ITransformJobProvider<TData>
        where TData : struct, ITransformJob
    {
        TData Data { get; }
        Transform Transform { get; }
    }
}
