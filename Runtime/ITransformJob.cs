using UnityEngine.Jobs;

namespace Gilzoide.EasyTransformJob
{
    public interface ITransformJob
    {
        void Process(TransformAccess transform, float deltaTime);
    }
}
