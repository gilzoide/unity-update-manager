#if HAVE_PERFORMANCE_TESTING
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gilzoide.EasyTransformJob.Tests.Performance
{
    public class ComparePerformance
    {
        [UnityTest, Performance]
        public IEnumerator PlainUpdate()
        {
            yield return RunTest<Rotate>();
        }

        [UnityTest, Performance]
        public IEnumerator ManagedUpdate()
        {
            yield return RunTest<RotateManaged>();
        }

        [UnityTest, Performance]
        public IEnumerator JobifiedUpdate()
        {
            yield return RunTest<RotateJobified>();
        }

        public IEnumerator RunTest<T>() where T : MonoBehaviour
        {
            var objects = new List<GameObject>();
            for (int i = 0; i < 1000; i++)
            {
                objects.Add(new GameObject(typeof(T).Name + " " + i, typeof(T)));
            }
            yield return Measure.Frames()
                // .WarmupCount(100)
                .MeasurementCount(1000)
                .Run();
            
            foreach (GameObject obj in objects)
            {
                Object.Destroy(obj);
            }
            yield return null;
        }
    }
}
#endif