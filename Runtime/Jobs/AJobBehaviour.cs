using UnityEngine;

namespace Gilzoide.UpdateManager.Jobs
{
    public abstract class AJobBehaviour<TData> : MonoBehaviour, ITransformJobUpdatable<TData>
        where TData : struct, IUpdateTransformJob
    {
        public virtual void OnEnable()
        {
            UpdateTransformJobManager<TData>.Instance.Register(this);
        }

        public virtual void OnDisable()
        {
            UpdateTransformJobManager<TData>.Instance.Unregister(this);
        }

        public Transform Transform => transform;
        public virtual TData InitialJobData => default;
        public TData JobData => UpdateTransformJobManager<TData>.Instance.GetData(this);
    }
}
