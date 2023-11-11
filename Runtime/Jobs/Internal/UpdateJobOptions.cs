using System;
using System.Reflection;
using Gilzoide.UpdateManager.Extensions;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public static class UpdateJobOptions
    {
        public const int DefaultBatchSize = 64;

        public static int GetBatchSize<TData>()
        {
            if (typeof(TData).GetCustomAttribute<JobBatchSizeAttribute>() is JobBatchSizeAttribute batchSizeAttribute)
            {
                return batchSizeAttribute.BatchSize;
            }
#pragma warning disable CS0618
            else if (typeof(TData).GetCustomAttribute<UpdateJobOptionsAttribute>() is UpdateJobOptionsAttribute options
                && options.BatchSize > 0)
            {
                return options.BatchSize;
            }
#pragma warning restore CS0618
            else
            {
                return DefaultBatchSize;
            }
        }

        public static bool GetReadOnlyTransformAccess<TData>()
        {
            return typeof(TData).GetCustomAttribute<ReadOnlyTransformAccessAttribute>() != null
#pragma warning disable CS0618
                || (typeof(TData).GetCustomAttribute<UpdateJobOptionsAttribute>() is UpdateJobOptionsAttribute options
                    && options.ReadOnlyTransforms);
#pragma warning restore CS0618
        }

        public static Type[] GetDependsOn<TData>()
        {
            if (typeof(TData).GetCustomAttribute<DependsOnAttribute>() is DependsOnAttribute dependsOn
                && dependsOn.DependencyTypes?.Length > 0)
            {
                return dependsOn.DependencyTypes;
            }
            else
            {
                return Array.Empty<Type>();
            }
        }

        public static IJobManager[] GetDependsOnManagers<TData>()
        {
            Type[] dependencyTypes = GetDependsOn<TData>();
            if (dependencyTypes.Length == 0)
            {
                return Array.Empty<IJobManager>();
            }

            var managers = new IJobManager[dependencyTypes.Length];
            for (int i = 0; i < dependencyTypes.Length; i++)
            {
                Type type = dependencyTypes[i];
                if (type.IsIUpdateJob())
                {
                    managers[i] = (IJobManager) typeof(UpdateJobManager<>).MakeGenericType(type).GetProperty("Instance").GetValue(null);
                }
                else if (type.IsIUpdateTransformJob())
                {
                    managers[i] = (IJobManager) typeof(UpdateTransformJobManager<>).MakeGenericType(type).GetProperty("Instance").GetValue(null);
                }
                else
                {
                    throw new ArgumentException(
                        $"Dependency type '{type}' must implement IUpdateJob or IUpdateTransformJob",
                        nameof(DependsOnAttribute.DependencyTypes)
                    );
                }
            }
            return managers;
        }

#if HAVE_BURST
        public static bool GetIsBurstCompiled<TData>()
        {
            return typeof(TData).ImplementsGenericInterface(typeof(IBurstUpdateJob<>))
                || typeof(TData).ImplementsGenericInterface(typeof(IBurstUpdateTransformJob<>));
        }
#endif
    }
}
