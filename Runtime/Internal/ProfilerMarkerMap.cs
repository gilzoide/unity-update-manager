#if (UNITY_EDITOR || DEVELOPMENT_BUILD) && !UPDATE_MANAGER_DISABLE_PROFILER_MARKERS
#define ENABLE_PROFILER_MARKERS
#endif
#if ENABLE_PROFILER_MARKERS
using System;
using System.Runtime.CompilerServices;
using Unity.Profiling;

namespace Gilzoide.UpdateManager.Internal
{
    public static class ProfilerMarkerMap
    {
        public static ProfilerMarker.AutoScope GetUpdate(object obj)
        {
            return Get(_profileMarkerCacheUpdate, nameof(IUpdatable.ManagedUpdate), obj);
        }

        public static ProfilerMarker.AutoScope GetLateUpdate(object obj)
        {
            return Get(_profileMarkerCacheLateUpdate, nameof(ILateUpdatable.ManagedLateUpdate), obj);
        }

        public static ProfilerMarker.AutoScope GetFixedUpdate(object obj)
        {
            return Get(_profileMarkerCacheFixedUpdate, nameof(IFixedUpdatable.ManagedFixedUpdate), obj);
        }

        private readonly static ConditionalWeakTable<Type, ProfilerMarkerClass> _profileMarkerCacheUpdate = new ConditionalWeakTable<Type, ProfilerMarkerClass>();
        private readonly static ConditionalWeakTable<Type, ProfilerMarkerClass> _profileMarkerCacheLateUpdate = new ConditionalWeakTable<Type, ProfilerMarkerClass>();
        private readonly static ConditionalWeakTable<Type, ProfilerMarkerClass> _profileMarkerCacheFixedUpdate = new ConditionalWeakTable<Type, ProfilerMarkerClass>();
    
        private static ProfilerMarker.AutoScope Get(ConditionalWeakTable<Type, ProfilerMarkerClass> cache, string method, object obj)
        {
            Type type = obj.GetType();
            if (!cache.TryGetValue(type, out ProfilerMarkerClass profilerMarker))
            {
                profilerMarker = new ProfilerMarkerClass(type, method);
                cache.Add(type, profilerMarker);
            }
            return profilerMarker.Auto();
        }

        private class ProfilerMarkerClass
        {
            private readonly ProfilerMarker _profilerMarker;

            public ProfilerMarkerClass(Type type, string method)
            {
                _profilerMarker = new ProfilerMarker(type.Name + "." + method + "()");
            }

            public ProfilerMarker.AutoScope Auto()
            {
                return _profilerMarker.Auto();
            }
        }
    }
}
#endif
