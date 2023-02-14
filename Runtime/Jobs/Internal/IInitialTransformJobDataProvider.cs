using UnityEngine;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public interface IInitialTransformJobDataProvider<TData> : IInitialJobDataProvider<TData>
    {
        Transform Transform { get; }
    }
}
