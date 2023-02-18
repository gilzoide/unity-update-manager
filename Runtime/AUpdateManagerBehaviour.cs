using UnityEngine;

namespace Gilzoide.UpdateManager
{
    /// <summary>
    /// <see cref="MonoBehaviour"/> abstract class with automatic registration in <see cref="UpdateManager"/>.
    /// </summary>
    /// <remarks>
    /// Instances will register themselves for updates in the <c>OnEnable</c> message and unregister in the <c>OnDisable</c> message.
    /// </remarks>
    /// <seealso cref="UpdateManager"/>
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

        /// <summary>
        /// Implement this for receiving managed updates from <see cref="UpdateManager"/>.
        /// </summary>
        /// <seealso cref="UpdateManager"/>
        public abstract void ManagedUpdate();
    }
}
