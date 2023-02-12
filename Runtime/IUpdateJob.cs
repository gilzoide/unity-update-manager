using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager
{
    public interface IUpdateJob
    {
        void Process(TransformAccess transform);
    }
}
