using UnityEngine;

namespace Gilzoide.UpdateManager.Jobs
{
    /// <summary>
    /// <see cref="MonoBehaviour"/> abstract class with automatic registration in <see cref="UpdateTransformJobManager{}"/>.
    /// </summary>
    /// <remarks>
    /// Instances will register themselves for job scheduling in the <c>OnEnable</c> message and unregister in the <c>OnDisable</c> message.
    /// </remarks>
    /// <seealso cref="UpdateTransformJobManager{}"/>
    public abstract class AJobBehaviour<TData> : MonoBehaviour, ITransformJobUpdatable<TData>
        where TData : struct, IUpdateTransformJob
    {
        /// <summary>
        /// Whether job data should be synchronized every frame.
        /// Only meaningful in subclasses implementing <see cref="IJobDataSynchronizer{}"/>.
        /// </summary>
        public virtual bool SynchronizeJobDataEveryFrame => true;

        protected virtual void OnEnable()
        {
            this.RegisterInManager(SynchronizeJobDataEveryFrame);
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
        /// <seealso cref="ITransformJobUpdatable{}.InitialJobData"/>
        public virtual TData InitialJobData => default;
        
        /// <summary>
        /// Shortcut for <c>UpdateTransformJobManager&lt;TData&gt;.Instance.GetData(this)</c>.
        /// </summary>
        /// <seealso cref="UpdateTransformJobManager{}.GetData"/>
        public TData JobData => this.GetJobData();

#if UNITY_EDITOR
        /// <summary>
        /// Whether job data should be synchronized in the <c>OnValidate</c> message.
        /// Only meaningful in subclasses implementing <see cref="IJobDataSynchronizer{}"/>.
        /// </summary>
        protected virtual bool SynchronizeJobDataOnValidate => true;

        /// <summary>
        /// Synchronizes job data on Play mode.
        /// Only meaningful in subclasses implementing <see cref="IJobDataSynchronizer{}"/>.
        /// </summary>
        protected virtual void OnValidate()
        {
            if (Application.isPlaying && SynchronizeJobDataOnValidate)
            {
                this.SynchronizeJobDataOnce();
            }
        }
#endif
    }

    /// <summary>
    /// Alias for <see cref="AJobBehaviour{}"/>.
    /// Pass <c>BurstUpdateTransformJob&lt;<typeparamref name="TData"/>&gt;</c> as <typeparamref name="TJob"/> to Burst compile your job.
    /// </summary>
    public abstract class AJobBehaviour<TData, TJob> : AJobBehaviour<TData>
        where TData : struct, IUpdateTransformJob
        where TJob : struct, IInternalUpdateTransformJob<TData>
    {
    }
}
