using System;
using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager.Jobs
{
    /// <summary>
    /// Add this to a managed <see cref="TransformAccess"/>-enabled job struct type to specify that its transform access is read-only.
    /// </summary>
    /// <remarks>
    /// Read-only transform jobs may be fully parallelized by Unity.
    /// Read-write transform jobs, on the other hand, are only parallelized for objects in hierarchies with different root objects.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Struct)]
    public class ReadOnlyTransformAccessAttribute : Attribute
    {
    }
}
