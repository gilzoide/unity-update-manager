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

        private readonly HashSet<IManagedUpdatable> _managedUpdatables = new HashSet<IManagedUpdatable>();
        private readonly List<IManagedUpdatable> _managedUpdatablesToAdd = new List<IManagedUpdatable>();
        private readonly List<IManagedUpdatable> _managedUpdatablesToRemove = new List<IManagedUpdatable>();

        void Update()
        {
            foreach (IManagedUpdatable updatable in _managedUpdatables)
            {
                updatable.ManagedUpdate();
            }
            
            _managedUpdatables.UnionWith(_managedUpdatablesToAdd);
            _managedUpdatablesToAdd.Clear();

            _managedUpdatables.ExceptWith(_managedUpdatablesToRemove);
            _managedUpdatablesToRemove.Clear();
        }

        public void RegisterUpdatable(IManagedUpdatable updatable)
        {
            _managedUpdatablesToAdd.Add(updatable);
        }

        public void UnregisterUpdatable(IManagedUpdatable updatable)
        {
            _managedUpdatablesToRemove.Add(updatable);
        }
    }
}
