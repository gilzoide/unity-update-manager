using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager.Jobs
{
    /// <summary>
    /// <see cref="TransformAccess"/>-enabled update job structs must implement this interface to be executed by <see cref="UpdateTransformJobManager{,}"/>.
    /// </summary>
    public interface IUpdateTransformJob
    {
        void Execute(TransformAccess transform);
    }
}
