#if (UNITY_EDITOR || DEVELOPMENT_BUILD) && !UPDATE_MANAGER_DISABLE_PROFILER_MARKERS
    #define ENABLE_PROFILER_MARKERS
#endif
using System;
using System.Collections.Generic;
using Unity.Profiling;

namespace Gilzoide.UpdateManager.Internal
{
    public static class ProfilerMarkerMap
    {
#if ENABLE_PROFILER_MARKERS
        public static ProfilerMarker.AutoScope Get(string method, object obj)
        {
            Type type = obj.GetType();
            var key = (type, method);
            if (!_profileMarkerCache.TryGetValue(key, out ProfilerMarker profilerMarker))
            {
                profilerMarker = new ProfilerMarker(type.Name + "." + method + "()");
                _profileMarkerCache[key] = profilerMarker;
            }
            return profilerMarker.Auto();
        }

        private readonly static Dictionary<(Type, string), ProfilerMarker> _profileMarkerCache = new Dictionary<(Type, string), ProfilerMarker>();
#else
        public static IDisposable Get(string method, object obj)
        {
            return null;
        }
#endif
    }
}
