using System.Collections;
using System.Collections.Generic;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gilzoide.UpdateManager.Tests.Performance
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
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
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