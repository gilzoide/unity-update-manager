using Unity.Collections;
using UnityEngine;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public static class NativeArrayExtensions
    {
        public static void SwapBack<T>(this NativeArray<T> array, int index) where T : struct
        {
            int lastIndex = array.Length - 1;
            if (lastIndex > 0 && lastIndex != index)
            {
                array[index] = array[array.Length - 1];
            }
        }

        public static NativeArray<T> WithCapacity<T>(this NativeArray<T> array, int capacity, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory) where T : struct
        {
            var newArray = new NativeArray<T>(capacity, allocator, options);
            if (array.IsCreated)
            {
                NativeArray<T>.Copy(array, newArray, Mathf.Min(array.Length, capacity));
            }
            return newArray;
        }

        public static void Realloc<T>(ref NativeArray<T> array, int capacity, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory) where T : struct
        {
            if (capacity == array.Length)
            {
                return;
            }

            NativeArray<T> newArray = array.WithCapacity(capacity, allocator, options);
            DisposeIfCreated(ref array);
            array = newArray;
        }

        public static void DisposeIfCreated<T>(ref NativeArray<T> array) where T : struct
        {
            if (array.IsCreated)
            {
                array.Dispose();
            }
        }
    }
}
