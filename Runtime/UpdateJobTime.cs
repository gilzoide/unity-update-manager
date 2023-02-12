using UnityEngine;

namespace Gilzoide.EasyTransformJob
{
    public class UpdateJobTime : IUpdatable
    {
        public float time { get; internal set; }
        public float deltaTime { get; internal set; }
        public float smoothDeltaTime { get; internal set; }
        public float unscaledDeltaTime { get; internal set; }
        public float realtimeSinceStartup { get; internal set; }
        public float timeSinceLevelLoad { get; internal set; }

        private int registerCount = 0;

        public static UpdateJobTime Instance => _instance != null ? _instance : (_instance = new UpdateJobTime());
        protected static UpdateJobTime _instance;

        public void RegisterUpdate()
        {
            if (registerCount == 0)
            {
                UpdateManager.Instance.RegisterUpdatable(this);
            }
            registerCount++;
        }

        public void UnregisterUpdate()
        {
            registerCount--;
            if (registerCount == 0)
            {
                UpdateManager.Instance.UnregisterUpdatable(this);
            }
        }

        public void ManagedUpdate()
        {
            time = Time.time;
            deltaTime = Time.deltaTime;
            smoothDeltaTime = Time.smoothDeltaTime;
            unscaledDeltaTime = Time.unscaledDeltaTime;
            realtimeSinceStartup = Time.realtimeSinceStartup;
            timeSinceLevelLoad = Time.timeSinceLevelLoad;
        }
    }
}
