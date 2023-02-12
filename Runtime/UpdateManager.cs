using System.Collections.Generic;
using UnityEngine;

namespace Gilzoide.UpdateManager
{
    public class UpdateManager : MonoBehaviour
    {
        public static UpdateManager Instance => (ApplicationUtils.IsQuitting || _instance != null) ? _instance : (_instance = CreateInstance());
        protected static UpdateManager _instance;

        private int _loopCounter;

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

        private void Update()
        {
            for (_loopCounter = 0; _loopCounter < _updatableObjects.Count; _loopCounter++)
            {
                _updatableObjects[_loopCounter].ManagedUpdate();
            }
        }

        public void RegisterUpdatable(IUpdatable updatable)
        {
            _updatableObjects.Add(updatable);
        }

        public void UnregisterUpdatable(IUpdatable updatable)
        {
            int index = _updatableObjects.IndexOf(updatable);
            if (index < 0)
            {
                return;
            }

            _updatableObjects.RemoveAt(index);
            if (_loopCounter >= index)
            {
                _loopCounter--;
            }
        }
    }
}
