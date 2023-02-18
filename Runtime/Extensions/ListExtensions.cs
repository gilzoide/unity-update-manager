using System.Collections.Generic;

namespace Gilzoide.UpdateManager.Extensions
{
    public static class ListExtensions
    {
        public static void RemoveAtSwapBack<T>(this IList<T> list, int index, out T swappedValue)
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

        public static void Swap<T>(this IList<T> list, int sourceIndex, int destinationIndex, out T newDestinationValue)
        {
            newDestinationValue = list[sourceIndex];
            list[sourceIndex] = list[destinationIndex];
            list[destinationIndex] = newDestinationValue;
        }
    }
}
