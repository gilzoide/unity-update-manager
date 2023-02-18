using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Gilzoide.UpdateManager.Tests
{
    public class TestUpdateManager
    {
        [UnityTest]
        public IEnumerator WhenUpdateRuns_AllObjectsShouldUpdate()
        {
            var counters = new List<Counter>();
            for (int frame = 0; frame < 10; frame++)
            {
                var counter = new Counter();
                counters.Add(counter);

                for (int i = 0; i < frame; i++)
                {
                    Assert.AreEqual(frame - i, counters[i].Count);
                }
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator WhenRegisteringObjectMultipleTimes_ShouldUpdateOnlyOnce()
        {
            var counter = new Counter();
            counter.RegisterInManager();
            counter.RegisterInManager();
            counter.RegisterInManager();

            yield return null;
            Assert.AreEqual(1, counter.Count);
        }

        [Test]
        public void WhenUnregisteringObject_WhenItWasntRegistered_ShouldBeNoop()
        {
            var counter = new Counter(false);
            counter.UnregisterInManager();
            counter.UnregisterInManager();
            counter.UnregisterInManager();
        }

        [UnityTest]
        public IEnumerator WhenAddingObjectsInsideManagedUpdate_NewObjectShouldBeUpdated()
        {
            var counter1 = new Counter();
            var counter2 = new Counter(false);
            counter1.AddCallback(2, counter2.RegisterInManager);

            yield return null;
            Assert.AreEqual(1, counter1.Count);
            Assert.AreEqual(0, counter2.Count);  // not added yet

            yield return null;
            Assert.AreEqual(2, counter1.Count);
            Assert.AreEqual(1, counter2.Count);  // added this frame, already updated
        }

        [UnityTest]
        public IEnumerator WhenRemovingObjectsInsideManagedUpdate_AllRemainingObjectsMustBeUpdated()
        {
            const int counterCount = 15;
            var counters = new List<Counter>();
            for (int i = 0; i < counterCount; i++)
            {
                counters.Add(new Counter());
            }

            // Frame 1: remove self
            counters[2].AddCallback(1, () =>
            {
                counters[2].UnregisterInManager();
                counters.RemoveAt(2);
            });

            // Frame 2: remove last object
            counters[3].AddCallback(2, () =>
            {
                counters[counters.Count - 1].UnregisterInManager();
                counters.RemoveAt(counters.Count - 1);
            });

            // Frame 3: remove object later in the list
            counters[4].AddCallback(3, () =>
            {
                counters[counters.Count - 2].UnregisterInManager();
                counters.RemoveAt(counters.Count - 2);
            });

            // Frame 4: remove object from list that already updated
            counters[5].AddCallback(4, () =>
            {
                counters[0].UnregisterInManager();
                counters.RemoveAt(0);
            });
            // Frame 5: repeat test
            counters[6].AddCallback(5, () =>
            {
                counters[0].UnregisterInManager();
                counters.RemoveAt(0);

                counters[0].UnregisterInManager();
                counters.RemoveAt(0);
            });
            
            for (int frame = 1; frame < 10; frame++)
            {
                yield return null;
                foreach (Counter counter in counters)
                {
                    Assert.AreEqual(frame, counter.Count);
                }
            }
        }

        #region Setup

        private class Counter : IUpdatable
        {
            public int Count = 0;
            private Dictionary<int, Action> _callbacks = new Dictionary<int, Action>();

            public Counter(bool autoregister = true)
            {
                if (autoregister)
                {
                    RegisterInManager();
                }
            }

            public void RegisterInManager()
            {
                _updateManager.Register(this);
            }

            public void UnregisterInManager()
            {
                _updateManager.Unregister(this);
            }

            public void ManagedUpdate()
            {
                Count++;
                if (_callbacks.TryGetValue(Count, out Action action))
                {
                    action?.Invoke();
                }
            }

            public void AddCallback(int count, Action action)
            {
                _callbacks[count] = action;
            }
        }

        private static UpdateManager _updateManager;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _updateManager = new GameObject(nameof(UpdateManager)).AddComponent<UpdateManager>();
        }

        [SetUp]
        public void SetUp()
        {
            _updateManager.Clear();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Object.DestroyImmediate(_updateManager.gameObject);
        }

        #endregion
    }
}
