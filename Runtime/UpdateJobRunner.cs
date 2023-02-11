using System.Collections.Generic;
using UnityEngine;

namespace Gilzoide.EasyTransformJob
{
    public class UpdateJobRunner : MonoBehaviour
    {
        public static UpdateJobRunner Instance => _instance != null ? _instance : (_instance = CreateInstance());
        protected static UpdateJobRunner _instance;

        private static UpdateJobRunner CreateInstance()
        {
            var gameObject = new GameObject(nameof(UpdateJobRunner));
            DontDestroyOnLoad(gameObject);
            return gameObject.AddComponent<UpdateJobRunner>();
        }

        private readonly List<IUpdateJobManager> _jobManagers = new List<IUpdateJobManager>();

        void Update()
        {
            UpdateJobTime.DeltaTime = Time.deltaTime;
            UpdateJobTime.SmoothDeltaTime = Time.smoothDeltaTime;
            UpdateJobTime.UnscaledDeltaTime = Time.unscaledDeltaTime;

            for (int i = 0; i < _jobManagers.Count; i++)
            {
                _jobManagers[i].Process();
            }
        }

        void OnDestroy()
        {
            for (int i = 0; i < _jobManagers.Count; i++)
            {
                _jobManagers[i].Dispose();
            }
        }

        public void RegisterJobManager(IUpdateJobManager manager)
        {
            _jobManagers.Add(manager);
        }

        public void UnregisterJobManager(IUpdateJobManager manager)
        {
            _jobManagers.Remove(manager);
        }
    }
}
