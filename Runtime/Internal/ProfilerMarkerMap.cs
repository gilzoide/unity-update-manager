#if (UNITY_EDITOR || DEVELOPMENT_BUILD) && !UPDATE_MANAGER_DISABLE_PROFILER_MARKERS
    #define ENABLE_PROFILER_MARKERS
#endif
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Profiling;

namespace Gilzoide.UpdateManager.Internal
{
    public static class ProfilerMarkerMap
    {
#if ENABLE_PROFILER_MARKERS
        public static ProfilerMarker.AutoScope Get(string method, object obj)
        {
            if (!_perMethodMap.TryGetValue(method, out TypeMap map))
            {
                map = new TypeMap(method);
                _perMethodMap.Add(method, map);
            }
            return map.Get(obj.GetType());
        }

        private static Dictionary<string, TypeMap> _perMethodMap = new Dictionary<string, TypeMap>();

        private class TypeMap
        {
            private readonly ConditionalWeakTable<Type, Value> _profileMarkerMap = new ConditionalWeakTable<Type, Value>();
            private string _method;

            public TypeMap(string method)
            {
                _method = method;
            }

            public ProfilerMarker.AutoScope Get(Type type)
            {
                if (!_profileMarkerMap.TryGetValue(type, out Value value))
                {
                    value = new Value(type, _method);
                    _profileMarkerMap.Add(type, value);
                }
                return value.ProfilerMarker.Auto();
            }

            private class Value
            {
                public Value(Type type, string method)
                {
                    ProfilerMarker = new ProfilerMarker(type.Name + "." + method + "()");
                }

                public ProfilerMarker ProfilerMarker;
            }
        }
#else
        public static IDisposable Get(string method, object obj)
        {
            return null;
        }
#endif
    }
}
