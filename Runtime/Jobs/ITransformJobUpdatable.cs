using UnityEngine;

namespace Gilzoide.UpdateManager.Jobs
{
    public interface ITransformJobUpdatable<TData>
        where TData : struct, IUpdateTransformJob
    {
        Transform Transform { get; }
        TData InitialJobData { get; }
    }
}
