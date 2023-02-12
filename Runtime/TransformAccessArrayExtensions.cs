using UnityEngine.Jobs;

namespace Gilzoide.EasyTransformJob
{
    public static class TransformAccessArrayExtensions
    {
        public static void DisposeIfCreated(ref TransformAccessArray transformArray)
        {
            if (transformArray.isCreated)
            {
                transformArray.Dispose();
            }
        }
    }
}
