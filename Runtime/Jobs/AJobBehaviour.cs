using UnityEngine;

namespace Gilzoide.UpdateManager.Jobs
{
    public abstract class AJobBehaviour<TData> : MonoBehaviour, ITransformJobUpdatable<TData>
        where TData : struct, IUpdateTransformJob
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
}
