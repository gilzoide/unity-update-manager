using System.Collections.Generic;
using UnityEngine;

namespace Gilzoide.EasyTransformJob
{
    public class UpdateJobRunner : IUpdatable
    {
        public static UpdateJobRunner Instance => _instance != null ? _instance : (_instance = new UpdateJobRunner());
        protected static UpdateJobRunner _instance;

        private readonly List<IUpdateJobManager> _jobManagers = new List<IUpdateJobManager>();

        public void ManagedUpdate()
        {
            UpdateJobTime.DeltaTime = Time.deltaTime;
            UpdateJobTime.SmoothDeltaTime = Time.smoothDeltaTime;
            UpdateJobTime.UnscaledDeltaTime = Time.unscaledDeltaTime;

            for (int i = 0; i < _jobManagers.Count; i++)
            {
                _jobManagers[i].Process();
            }
        }

        public void RegisterJobManager(IUpdateJobManager manager)
        {
            if (_jobManagers.Count == 0)
            {
                UpdateManager.Instance.RegisterUpdatable(this);
            }
            _jobManagers.Add(manager);
        }

        public void UnregisterJobManager(IUpdateJobManager manager)
        {
            _jobManagers.Remove(manager);
            if (_jobManagers.Count == 0)
            {
                UpdateManager.Instance.UnregisterUpdatable(this);
            }
        }
    }
}
