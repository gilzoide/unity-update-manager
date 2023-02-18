using Gilzoide.UpdateManager.Jobs.Internal;
using UnityEngine;

namespace Gilzoide.UpdateManager.Jobs
{
    /// <summary>
    /// <see cref="MonoBehaviour"/> abstract class with automatic registration in <see cref="UpdateTransformJobManager{,}"/>.
    /// </summary>
    /// <remarks>
    /// Instances will register themselves for job scheduling in the <c>OnEnable</c> message and unregister in the <c>OnDisable</c> message.
    /// </remarks>
    /// <seealso cref="UpdateTransformJobManager{,}"/>
    public abstract class AJobBehaviour<TData, TJob> : MonoBehaviour, ITransformJobUpdatable<TData, TJob>
        where TData : struct, IUpdateTransformJob
        where TJob : struct, IInternalUpdateTransformJob<TData>
    {
        protected virtual void OnEnable()
        {
            this.RegisterInManager();
        }

        protected virtual void OnDisable()
        {
            this.UnregisterInManager();
        }

        public Transform Transform => transform;

        /// <summary>
        /// Returns the job data for the first job run.
        /// </summary>
        /// <remarks>
        /// Override this in child classes to use data other than <see langword="default"/> in the first scheduled job run.
        /// </remarks>
        /// <seealso cref="ITransformJobUpdatable{,}.InitialJobData"/>
        public virtual TData InitialJobData => default;
        
        /// <summary>
        /// Shortcut for <c>UpdateTransformJobManager&lt;TData, TJob&gt;.Instance.GetData(this)</c>.
        /// </summary>
        /// <seealso cref="UpdateTransformJobManager{,}.GetData"/>
        public TData JobData => this.GetJobData();
    }

    /// <summary>
    /// Alias for <see cref="AJobBehaviour{,}"/> that defaults to using jobs that are not Burst compilable.
    /// </summary>
    public abstract class AJobBehaviour<TData> : AJobBehaviour<TData, UpdateTransformJob<TData>>
        where TData : struct, IUpdateTransformJob
    {
    }
}
