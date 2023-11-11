using Gilzoide.UpdateManager.Jobs.Internal;
using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager.Jobs
{
    /// <summary>
    /// <see cref="TransformAccess"/>-enabled update job structs must implement this interface to be executed by <see cref="UpdateTransformJobManager{}"/>.
    /// </summary>
    public interface IUpdateTransformJob
    {
        void Execute(TransformAccess transform);
    }

    /// <summary>
    /// A Burst-enabled version of <see cref="IUpdateTransformJob"/>.
    /// Implement this if you want to have your job compiled by Burst.
    /// </summary>
    /// <typeparam name="TBurstJob">
    ///   A concrete version of <see cref="BurstUpdateTransformJob{}"/> using the job type itself as type parameter.
    /// </typeparam>
    public interface IBurstUpdateTransformJob<TBurstJob> : IUpdateTransformJob
        where TBurstJob : IInternalBurstUpdateTransformJob
    {        
    }
}
