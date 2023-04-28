using System;
using System.Collections.Generic;
using Gilzoide.UpdateManager.Extensions;
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

        private readonly List<IUpdatable> _updatableObjects = new List<IUpdatable>();
        private readonly Dictionary<IUpdatable, int> _updatableIndexMap = new Dictionary<IUpdatable, int>();
        private int _loopIndex;

        private void Update()
        {
            UpdateJobTime.InstanceRef.Refresh();
            for (_loopIndex = 0; _loopIndex < _updatableObjects.Count; _loopIndex++)
            {
                try
                {
                    _updatableObjects[_loopIndex].ManagedUpdate();
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                }
            }
            _loopIndex = 0;
        }

        /// <summary>
        /// Register <paramref name="updatable"/> to be updated every frame.
        /// </summary>
        /// <remarks>
        /// Registering updatable objects is O(1).
        /// Registering an object more than once is a no-op.
        /// </remarks>
        public void Register(IUpdatable updatable)
        {
            if (_updatableIndexMap.ContainsKey(updatable))
            {
                return;
            }

            _updatableObjects.Add(updatable);
            _updatableIndexMap.Add(updatable, _updatableObjects.Count - 1);
        }

        /// <summary>
        /// Unregister <paramref name="updatable"/>, so it is not updated every frame anymore.
        /// </summary>
        /// <remarks>
        /// Unregistering updatable objects is O(1).
        /// Unregistering an object that wasn't registered is a no-op.
        /// </remarks>
        public void Unregister(IUpdatable updatable)
        {
            if (!_updatableIndexMap.TryGetValue(updatable, out int indexToRemove))
            {
                return;
            }

            _updatableIndexMap.Remove(updatable);

            // If removing the object that was just updated, make sure the
            // new object swapped back to this index gets updated as well
            if (indexToRemove == _loopIndex)
            {
                _loopIndex--;
            }
            // If removing an object that was already updated while the loop is
            // still running, swap it with current loop index to make sure we
            // still update the last element that will be swapped back later
            else if (indexToRemove < _loopIndex)
            {
                _updatableObjects.Swap(_loopIndex, indexToRemove, out IUpdatable swappedUpdatable);
                _updatableIndexMap[swappedUpdatable] = indexToRemove;
                indexToRemove = _loopIndex;
                _loopIndex--;
            }

            _updatableObjects.RemoveAtSwapBack(indexToRemove, out IUpdatable swappedBack);
            if (swappedBack != null)
            {
                _updatableIndexMap[swappedBack] = indexToRemove;
            }
        }

        /// <summary>
        /// Unregisters all updatable objects at once.
        /// </summary>
        public void Clear()
        {
            _updatableObjects.Clear();
            _updatableIndexMap.Clear();
        }
    }
}
