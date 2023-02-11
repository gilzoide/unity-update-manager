using UnityEngine.Jobs;

namespace Gilzoide.EasyTransformJob
{
    public interface IUpdateJob
    {
        void Process(TransformAccess transform);
    }
}
