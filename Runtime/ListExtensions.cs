using System.Collections.Generic;

namespace Gilzoide.EasyTransformJob
{
    public static class ListExtensions
    {
        public static void RemoveAtSwapBack<T>(this List<T> list, int index, out T swappedValue)
        {
            int lastIndex = list.Count - 1;
            if (lastIndex > 0 && lastIndex != index)
            {
                swappedValue = list[index] = list[lastIndex];
            }
            else
            {
                swappedValue = default;
            }
            list.RemoveAt(lastIndex);
        }
    }
}
