namespace Gilzoide.UpdateManager.Jobs
{
    /// <summary>
    /// Struct for accessing <see cref="UnityEngine.Time"/> properties from within jobs or other multithreaded code.
    /// </summary>
    /// <remarks>
    /// All properties are available from Burst-compilable jobs.
    /// </remarks>
    public struct UpdateJobTime
    {
        /// <summary>Cached value for <see cref="UnityEngine.Time.time"/> from current running frame</summary>
        public static float time => InstanceRef.Time;
        /// <summary>Cached value for <see cref="UnityEngine.Time.deltaTime"/> from current running frame</summary>
        public static float deltaTime => InstanceRef.DeltaTime;
        /// <summary>Cached value for <see cref="UnityEngine.Time.smoothDeltaTime"/> from current running frame</summary>
        public static float smoothDeltaTime => InstanceRef.SmoothDeltaTime;
        /// <summary>Cached value for <see cref="UnityEngine.Time.unscaledDeltaTime"/> from current running frame</summary>
        public static float unscaledDeltaTime => InstanceRef.UnscaledDeltaTime;
        /// <summary>Cached value for <see cref="UnityEngine.Time.realtimeSinceStartup"/> from current running frame</summary>
        /// <remarks>Contrary to <see cref="UnityEngine.Time.realtimeSinceStartup"/>, this property will return the same value if called twice during the same frame.</remarks>
        public static float realtimeSinceStartup => InstanceRef.RealtimeSinceStartup;
        /// <summary>Cached value for <see cref="UnityEngine.Time.timeSinceLevelLoad"/> from current running frame</summary>
        public static float timeSinceLevelLoad => InstanceRef.TimeSinceLevelLoad;
        /// <summary>Cached value for <see cref="UnityEngine.Time.frameCount"/> from current running frame</summary>
        public static int frameCount => InstanceRef.FrameCount;

        /// <summary>Cached value for <see cref="UnityEngine.Time.time"/> from current running frame</summary>
        public float Time { get; private set; }
        /// <summary>Cached value for <see cref="UnityEngine.Time.deltaTime"/> from current running frame</summary>
        public float DeltaTime { get; private set; }
        /// <summary>Cached value for <see cref="UnityEngine.Time.smoothDeltaTime"/> from current running frame</summary>
        public float SmoothDeltaTime { get; private set; }
        /// <summary>Cached value for <see cref="UnityEngine.Time.unscaledDeltaTime"/> from current running frame</summary>
        public float UnscaledDeltaTime { get; private set; }
        /// <summary>Cached value for <see cref="UnityEngine.Time.realtimeSinceStartup"/> from current running frame</summary>
        /// <remarks>Contrary to <see cref="UnityEngine.Time.realtimeSinceStartup"/>, this property will return the same value if called twice during the same frame.</remarks>
        public float RealtimeSinceStartup { get; private set; }
        /// <summary>Cached value for <see cref="UnityEngine.Time.timeSinceLevelLoad"/> from current running frame</summary>
        public float TimeSinceLevelLoad { get; private set; }
        /// <summary>Cached value for <see cref="UnityEngine.Time.frameCount"/> from current running frame</summary>
        public int FrameCount { get; private set; }

        public static UpdateJobTime Instance => InstanceRef;

#if HAVE_BURST
        internal static ref UpdateJobTime InstanceRef => ref Unity.Burst.SharedStatic<UpdateJobTime>.GetOrCreate<UpdateJobTime>().Data;
#else
        internal static UpdateJobTime InstanceRef;
#endif

        internal void Refresh()
        {
            Time = UnityEngine.Time.time;
            DeltaTime = UnityEngine.Time.deltaTime;
            SmoothDeltaTime = UnityEngine.Time.smoothDeltaTime;
            UnscaledDeltaTime = UnityEngine.Time.unscaledDeltaTime;
            RealtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
            TimeSinceLevelLoad = UnityEngine.Time.timeSinceLevelLoad;
            FrameCount = UnityEngine.Time.frameCount;
        }
    }
}
