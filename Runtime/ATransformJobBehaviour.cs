using UnityEngine;

namespace Gilzoide.EasyTransformJob
{
    public abstract class ATransformJobBehaviour<TData> : MonoBehaviour, ITransformJobProvider<TData>
        where TData : struct, ITransformJob
    {
        public virtual void OnEnable()
        {
            TransformJobManager<TData>.Instance.AddProvider(this);
        }

        public virtual void OnDisable()
        {
            TransformJobManager<TData>.Instance.RemoveProvider(this);
        }

        public abstract TData Data { get; }

        public Transform Transform => transform;
    }
}
