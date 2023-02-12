using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager.Jobs
{
    public interface IUpdateJob
    {
        void Process(TransformAccess transform);
    }
}
