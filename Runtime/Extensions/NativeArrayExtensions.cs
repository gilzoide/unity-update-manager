using Unity.Collections;

namespace Gilzoide.UpdateManager
{
    public static class NativeArrayExtensions
    {
        public static void DisposeIfCreated<T>(this ref NativeArray<T> array)
            where T : struct
        {
            if (array.IsCreated)
            {
                array.Dispose();
            }
        }
    }
}
