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
            yield return RunTest<RotateJobified>(useRootTransform: true);
        }
        
        [UnityTest, Performance]
        public IEnumerator JobifiedUpdateParallel()
        {
            yield return RunTest<RotateJobified>();
        }

        public IEnumerator RunTest<T>(bool useRootTransform = false) where T : MonoBehaviour
        {
            Transform rootTransform = useRootTransform ? new GameObject("root").transform : null;
            var objects = new List<GameObject>();
            for (int i = 0; i < 1000; i++)
            {
                GameObject gameObject = new GameObject(typeof(T).Name + " " + i, typeof(T));
                gameObject.transform.SetParent(rootTransform);
                objects.Add(gameObject);
            }
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            yield return Measure.Frames()
                .MeasurementCount(1000)
                .Run();
            
            foreach (GameObject obj in objects)
            {
                Object.Destroy(obj);
            }
            if (rootTransform)
            {
                Object.Destroy(rootTransform.gameObject);
            }
            yield return null;
        }
    }
}