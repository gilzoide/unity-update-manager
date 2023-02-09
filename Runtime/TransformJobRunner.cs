using System.Collections.Generic;
using UnityEngine;

namespace Gilzoide.EasyTransformJob
{
    public class TransformJobRunner : MonoBehaviour
    {
        public static TransformJobRunner Instance => _instance != null ? _instance : (_instance = CreateInstance());
        protected static TransformJobRunner _instance;

        private static TransformJobRunner CreateInstance()
        {
            var gameObject = new GameObject(nameof(TransformJobRunner));
            DontDestroyOnLoad(gameObject);
            return gameObject.AddComponent<TransformJobRunner>();
        }

        private readonly List<ITransformJobManager> _jobManagers = new List<ITransformJobManager>();

        void Update()
        {
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

        public void RegisterJobManager(ITransformJobManager manager)
        {
            _jobManagers.Add(manager);
        }
    }
}
