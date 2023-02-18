using System;
using Gilzoide.UpdateManager.Extensions;

namespace Gilzoide.UpdateManager.Jobs
{
    /// <summary>
    /// Add this to a managed job struct type for declaring dependencies between managed update jobs.
    /// </summary>
    /// <remarks>
    /// Dependency types must be struct types that implement either <see cref="IUpdateJob"/> or <see cref="IUpdateTransformJob"/>.
    /// </remarks>
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
                if (!type.IsValueType)
                {
                    throw new ArgumentException(
                        $"Dependency type must be a struct type: '{type}'",
                        nameof(dependencyTypes)
                    );
                }
                if (!type.IsIUpdateJob() && !type.IsIUpdateTransformJob())
                {
                    throw new ArgumentException(
                        $"Dependency type must implement IUpdateJob or IUpdateTransformJob: '{type}'",
                        nameof(dependencyTypes)
                    );
                }
            }
        }
    }
}
