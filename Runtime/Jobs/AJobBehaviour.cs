using Gilzoide.UpdateManager.Jobs.Internal;
using UnityEngine;

namespace Gilzoide.UpdateManager.Jobs
{
    public abstract class AJobBehaviour<TData, TJob> : MonoBehaviour, ITransformJobUpdatable<TData, TJob>
        where TData : struct, IUpdateTransformJob
        where TJob : struct, IInternalUpdateTransformJob<TData>
    {
        public virtual void OnEnable()
        {
            this.RegisterInManager();
        }

        public virtual void OnDisable()
        {
            this.UnregisterInManager();
        }

        public Transform Transform => transform;
        public virtual TData InitialJobData => default;
        public TData JobData => this.GetJobData();
    }

    public abstract class AJobBehaviour<TData> : AJobBehaviour<TData, UpdateTransformJob<TData>>
        where TData : struct, IUpdateTransformJob
    {
    }
}
