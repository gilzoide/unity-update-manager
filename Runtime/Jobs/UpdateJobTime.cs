using UnityTime = UnityEngine.Time;

namespace Gilzoide.UpdateManager.Jobs
{
    public struct UpdateJobTime
    {
        public static float time => InstanceRef.Time;
        public static float deltaTime => InstanceRef.DeltaTime;
        public static float smoothDeltaTime => InstanceRef.SmoothDeltaTime;
        public static float unscaledDeltaTime => InstanceRef.UnscaledDeltaTime;
        public static float realtimeSinceStartup => InstanceRef.RealtimeSinceStartup;
        public static float timeSinceLevelLoad => InstanceRef.TimeSinceLevelLoad;

        public float Time { get; private set; }
        public float DeltaTime { get; private set; }
        public float SmoothDeltaTime { get; private set; }
        public float UnscaledDeltaTime { get; private set; }
        public float RealtimeSinceStartup { get; private set; }
        public float TimeSinceLevelLoad { get; private set; }

        public static UpdateJobTime Instance => InstanceRef;

#if HAVE_BURST
        internal static ref UpdateJobTime InstanceRef => ref Unity.Burst.SharedStatic<UpdateJobTime>.GetOrCreate<UpdateJobTime>().Data;
#else
        internal static UpdateJobTime InstanceRef;
#endif

        internal void Refresh()
        {
            Time = UnityTime.time;
            DeltaTime = UnityTime.deltaTime;
            SmoothDeltaTime = UnityTime.smoothDeltaTime;
            UnscaledDeltaTime = UnityTime.unscaledDeltaTime;
            RealtimeSinceStartup = UnityTime.realtimeSinceStartup;
            TimeSinceLevelLoad = UnityTime.timeSinceLevelLoad;
        }
    }
}
