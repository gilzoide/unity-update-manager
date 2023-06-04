using System;
using System.Collections;
using System.Collections.Generic;

namespace Gilzoide.UpdateManager
{
    public class SortedList<T> : IComparer<T>, IEnumerable<T>
        where T : IComparable<T>
    {
        private List<T> _list = new List<T>();

        public int Count => _list.Count;

        public bool Add(T value)
        {
            int index = _list.BinarySearch(value, this);
            if (index < 0)
            {
                _list.Insert(~index, value);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Remove(T value)
        {
            int index = _list.BinarySearch(value, this);
            if (index >= 0)
            {
                _list.RemoveAt(index);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Contains(T value)
        {
            int index = _list.BinarySearch(value, this);
            return index >= 0;
        }

        public void Clear()
        {
            _list.Clear();
        }
        
        public List<T>.Enumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual int Compare(T x, T y)
        {
            return x.CompareTo(y);
        }
    }

    public class ReversedSortedList<T> : SortedList<T>
        where T : IComparable<T>
    {
        public override int Compare(T x, T y)
        {
            return -x.CompareTo(y);
        }
    }
}
