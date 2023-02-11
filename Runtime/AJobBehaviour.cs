using UnityEngine;

namespace Gilzoide.EasyTransformJob
{
    public abstract class AJobBehaviour<TData> : MonoBehaviour
        where TData : struct, IUpdateJob
    {
        public virtual void OnEnable()
        {
            UpdateJobManager<TData>.Instance.AddProvider(this);
        }

        public virtual void OnDisable()
        {
            UpdateJobManager<TData>.Instance.RemoveProvider(this);
        }

        public abstract TData Data { get; set; }
    }
}
