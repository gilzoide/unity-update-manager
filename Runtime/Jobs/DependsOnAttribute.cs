using System;
using Gilzoide.UpdateManager.Jobs.Internal;

namespace Gilzoide.UpdateManager.Jobs
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class DependsOnAttribute : Attribute
    {
        public Type[] DependencyTypes { get; private set; }

        public DependsOnAttribute(params Type[] dependencyTypes)
        {
            AssertUpdateJobTypes(dependencyTypes);
            DependencyTypes = dependencyTypes;
        }

        public static void AssertUpdateJobTypes(params Type[] dependencyTypes)
        {
            foreach (Type type in dependencyTypes)
            {
                if (!type.IsIUpdateJob() && !type.IsIUpdateTransformJob())
                {
                    throw new ArgumentException(
                        $"Dependency type '{type}' must implement IUpdateJob or IUpdateTransformJob",
                        nameof(dependencyTypes)
                    );
                }
            }
        }
    }
}
