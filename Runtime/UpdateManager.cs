using System;
using Gilzoide.UpdateManager.Internal;
using Gilzoide.UpdateManager.Jobs;
using UnityEngine;

namespace Gilzoide.UpdateManager
{
    /// <summary>
    /// Singleton MonoBehaviour that calls <see cref="IUpdatable.ManagedUpdate"/> to registered <see cref="IUpdatable"/> objects every frame.
    /// </summary>
    /// <remarks>
    /// Any C# object can be registered for updates, including MonoBehaviours, pure C# classes and structs, as long as they implement <see cref="IUpdatable"/>.
    /// <br/>
    /// This class doesn't implement any execution order mechanism, so don't rely on <see cref="IUpdatable.ManagedUpdate"/> methods being executed in any order.
    /// In fact, the order of executed methods might change during the lifetime of the UpdateManager.
    /// </remarks>
    public class UpdateManager : MonoBehaviour
    {
        /// <summary>Get or create the singleton instance</summary>
        public static UpdateManager Instance => (ApplicationUtils.IsQuitting || _instance != null) ? _instance : (_instance = CreateInstance());
        protected static UpdateManager _instance;

        private static UpdateManager CreateInstance()
        {
            var gameObject = new GameObject(nameof(UpdateManager))
            {
                hideFlags = HideFlags.DontSave,
            };
            DontDestroyOnLoad(gameObject);
            return gameObject.AddComponent<UpdateManager>();
        }

        private readonly FastRemoveList<IUpdatable> _updatableObjects = new FastRemoveList<IUpdatable>();
        private readonly FastRemoveList<ILateUpdatable> _lateUpdatableObjects = new FastRemoveList<ILateUpdatable>();
        private readonly FastRemoveList<IFixedUpdatable> _fixedUpdatableObjects = new FastRemoveList<IFixedUpdatable>();

        private void Update()
        {
            UpdateJobTime.InstanceRef.Refresh();
            using (var enumerator = _updatableObjects.GetEnumerator())
            while (enumerator.MoveNext())
            {
                try
                {
                    enumerator.Current.ManagedUpdate();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private void LateUpdate()
        {
            using (var enumerator = _lateUpdatableObjects.GetEnumerator())
            while (enumerator.MoveNext())
            {
                try
                {
                    enumerator.Current.ManagedLateUpdate();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private void FixedUpdate()
        {
            using (var enumerator = _fixedUpdatableObjects.GetEnumerator())
            while (enumerator.MoveNext())
            {
                try
                {
                    enumerator.Current.ManagedFixedUpdate();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        /// <summary>
        /// Register <paramref name="obj"/> to be updated every frame.
        /// </summary>
        /// <remarks>
        /// Registering updatable objects is O(1).
        /// Registering an object more than once is a no-op.
        /// </remarks>
        public void Register(IManagedObject obj)
        {
            if (obj is IUpdatable updatable)
            {
                _updatableObjects.Add(updatable);
            }
            if (obj is ILateUpdatable lateUpdatable)
            {
                _lateUpdatableObjects.Add(lateUpdatable);
            }
            if (obj is IFixedUpdatable fixedUpdatable)
            {
                _fixedUpdatableObjects.Add(fixedUpdatable);
            }
        }

        /// <summary>
        /// Unregister <paramref name="updatable"/>, so it is not updated every frame anymore.
        /// </summary>
        /// <remarks>
        /// Unregistering updatable objects is O(1).
        /// Unregistering an object that wasn't registered is a no-op.
        /// </remarks>
        public void Unregister(IManagedObject obj)
        {
            if (obj is IUpdatable updatable)
            {
                _updatableObjects.Remove(updatable);
            }
            if (obj is ILateUpdatable lateUpdatable)
            {
                _lateUpdatableObjects.Remove(lateUpdatable);
            }
            if (obj is IFixedUpdatable fixedUpdatable)
            {
                _fixedUpdatableObjects.Remove(fixedUpdatable);
            }
        }

        /// <summary>
        /// Unregisters all updatable objects at once.
        /// </summary>
        public void Clear()
        {
            _updatableObjects.Clear();
            _lateUpdatableObjects.Clear();
            _fixedUpdatableObjects.Clear();
        }
    }
}
