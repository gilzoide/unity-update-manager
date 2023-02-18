using System;
using Gilzoide.UpdateManager.Jobs;

namespace Gilzoide.UpdateManager.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsIUpdateJob(this Type type)
        {
            return typeof(IUpdateJob).IsAssignableFrom(type);
        }

        public static bool IsIUpdateTransformJob(this Type type)
        {
            return typeof(IUpdateTransformJob).IsAssignableFrom(type);
        }
    }
}
