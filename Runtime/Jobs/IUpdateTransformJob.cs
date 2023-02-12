using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager.Jobs
{
    public interface IUpdateTransformJob
    {
        void Execute(TransformAccess transform);
    }
}
