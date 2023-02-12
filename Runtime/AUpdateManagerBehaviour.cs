using UnityEngine;

namespace Gilzoide.UpdateManager
{
    public abstract class AUpdateManagerBehaviour : MonoBehaviour, IUpdatable
    {
        protected virtual void OnEnable()
        {
            this.RegisterInManager();
        }

        protected virtual void OnDisable()
        {
            this.UnregisterInManager();
        }

        public abstract void ManagedUpdate();
    }
}
