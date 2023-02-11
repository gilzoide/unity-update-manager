using UnityEngine;

namespace Gilzoide.EasyTransformJob
{
    public abstract class AUpdateManagerBehaviour : MonoBehaviour, IManagedUpdatable
    {
        protected virtual void OnEnable()
        {
            UpdateManager.Instance.RegisterUpdatable(this);
        }

        protected virtual void OnDisable()
        {
            UpdateManager.Instance.UnregisterUpdatable(this);
        }

        public abstract void ManagedUpdate();
    }
}
