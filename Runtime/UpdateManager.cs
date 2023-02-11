using System.Collections.Generic;
using UnityEngine;

namespace Gilzoide.EasyTransformJob
{
    public class UpdateManager : MonoBehaviour
    {
        public static UpdateManager Instance => (ApplicationUtils.IsQuitting || _instance != null) ? _instance : (_instance = CreateInstance());
        protected static UpdateManager _instance;

        private static UpdateManager CreateInstance()
        {
            var gameObject = new GameObject(nameof(UpdateManager));
            DontDestroyOnLoad(gameObject);
            return gameObject.AddComponent<UpdateManager>();
        }

        private readonly List<IManagedUpdatable> _managedUpdatables = new List<IManagedUpdatable>();
        private readonly HashSet<IManagedUpdatable> _managedUpdatablesToRemove = new HashSet<IManagedUpdatable>();

        void Update()
        {
            for (int i = 0; i < _managedUpdatables.Count; i++)
            {
                IManagedUpdatable updatable = _managedUpdatables[i];
                if (!_managedUpdatablesToRemove.Contains(updatable))
                {
                    updatable.ManagedUpdate();
                }
            }

            if (_managedUpdatablesToRemove.Count > 0)
            {
                _managedUpdatables.RemoveAll(_managedUpdatablesToRemove.Contains);
                _managedUpdatablesToRemove.Clear();
            }
        }

        public void RegisterUpdatable(IManagedUpdatable updatable)
        {
            _managedUpdatables.Add(updatable);
        }

        public void UnregisterUpdatable(IManagedUpdatable updatable)
        {
            _managedUpdatablesToRemove.Add(updatable);
        }
    }
}
