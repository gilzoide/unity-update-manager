using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager.Extensions
{
    public static class TransformAccessArrayExtensions
    {
        public static void DisposeIfCreated(this ref TransformAccessArray transformArray)
        {
            if (transformArray.isCreated)
            {
                transformArray.Dispose();
            }
        }
    }
}
