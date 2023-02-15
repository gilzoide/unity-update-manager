using System;

namespace Gilzoide.UpdateManager.Jobs.Internal
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
