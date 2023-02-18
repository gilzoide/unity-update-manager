using System.Collections.Generic;
using Gilzoide.UpdateManager.Extensions;
using Gilzoide.UpdateManager.Jobs;
using UnityEngine;

namespace Gilzoide.UpdateManager
{
    public class UpdateManager : MonoBehaviour
    {
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
            try
            {
                for (_loopIndex = 0; _loopIndex < _updatableObjects.Count; _loopIndex++)
                {
                    _updatableObjects[_loopIndex].ManagedUpdate();
                }
            }
            finally
            {
                _loopIndex = 0;
            }
        }

        public void Register(IUpdatable updatable)
        {
            if (_updatableIndexMap.ContainsKey(updatable))
            {
                return;
            }

            _updatableObjects.Add(updatable);
            _updatableIndexMap.Add(updatable, _updatableObjects.Count - 1);
        }

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
    }
}
