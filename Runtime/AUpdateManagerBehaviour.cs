using UnityEngine;

namespace Gilzoide.UpdateManager
{
    public abstract class AUpdateManagerBehaviour : MonoBehaviour, IUpdatable
    {
        protected virtual void OnEnable()
        {
            UpdateManager.Instance.Register(this);
        }

        protected virtual void OnDisable()
        {
            UpdateManager.Instance.Unregister(this);
        }

        public abstract void ManagedUpdate();
    }
}
