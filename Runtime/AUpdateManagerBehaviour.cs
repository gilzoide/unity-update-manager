using UnityEngine;

namespace Gilzoide.UpdateManager
{
    public abstract class AUpdateManagerBehaviour : MonoBehaviour, IUpdatable
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
