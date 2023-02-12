using System.Collections.Generic;
using UnityEngine;

namespace Gilzoide.UpdateManager
{
    public class ObjectComparer : IComparer<Object>
    {
        public static readonly ObjectComparer Instance = new ObjectComparer();

        public int Compare(Object x, Object y)
        {
            return x.GetInstanceID() - y.GetInstanceID();
        }
    }
}
