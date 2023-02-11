using UnityEngine;

namespace Gilzoide.EasyTransformJob
{
    public abstract class AUpdateManagerBehaviour : MonoBehaviour, IManagedUpdatable
    {
        protected virtual void OnEnable()
        {
            UpdateManager.Instance.RegisterBehaviour(this);
        }

        protected virtual void OnDisable()
        {
            UpdateManager.Instance.UnregisterBehaviour(this);
        }

        public abstract void ManagedUpdate();
    }
}
