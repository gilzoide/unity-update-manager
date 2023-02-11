using System.Collections.Generic;
using UnityEngine;

namespace Gilzoide.EasyTransformJob
{
    public class UpdateManager : MonoBehaviour
    {
        public static UpdateManager Instance => _instance != null ? _instance : (_instance = CreateInstance());
        protected static UpdateManager _instance;

        private static UpdateManager CreateInstance()
        {
            var gameObject = new GameObject(nameof(UpdateManager));
            DontDestroyOnLoad(gameObject);
            return gameObject.AddComponent<UpdateManager>();
        }

        private readonly List<AUpdateManagerBehaviour> _managedUpdatables = new List<AUpdateManagerBehaviour>();

        void Update()
        {
            for (int i = 0; i < _managedUpdatables.Count; i++)
            {
                _managedUpdatables[i].ManagedUpdate();
            }
        }

        public void RegisterBehaviour(AUpdateManagerBehaviour updatable)
        {
            int index = _managedUpdatables.BinarySearch(updatable, ObjectComparer.Instance);
            if (index < 0)
            {
                _managedUpdatables.Insert(~index, updatable);
            }
        }

        public void UnregisterBehaviour(AUpdateManagerBehaviour updatable)
        {
            int index = _managedUpdatables.BinarySearch(updatable, ObjectComparer.Instance);
            if (index >= 0)
            {
                _managedUpdatables.RemoveAt(index);
            }
        }
    }
}
