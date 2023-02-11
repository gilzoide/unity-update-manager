using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gilzoide.EasyTransformJob
{
    public static class ListExtensions
    {
        public static int AddSorted<T>(this List<T> list, T value)
        {
            int index = list.BinarySearch(value);
            if (index < 0)
            {
                index = ~index;
                list.Insert(index, value);
                return index;
            }
            else
            {
                return -1;
            }
        }

        public static int AddSorted<T>(this List<T> list, T value, IComparer<T> comparer)
        {
            int index = list.BinarySearch(value, comparer);
            if (index < 0)
            {
                index = ~index;
                list.Insert(index, value);
                return index;
            }
            else
            {
                return -1;
            }
        }

        public static bool RemoveSorted<T>(this List<T> list, T value)
        {
            int index = list.BinarySearch(value);
            if (index >= 0)
            {
                list.RemoveAt(index);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool RemoveSorted<T>(this List<T> list, T value, IComparer<T> comparer)
        {
            int index = list.BinarySearch(value, comparer);
            if (index >= 0)
            {
                list.RemoveAt(index);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
