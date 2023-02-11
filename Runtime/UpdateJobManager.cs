using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace Gilzoide.EasyTransformJob
{
    public class UpdateJobManager<TData> : IUpdateJobManager
        where TData : struct, IUpdateJob
    {
        public static UpdateJobManager<TData> Instance => _instance != null ? _instance : (_instance = new UpdateJobManager<TData>());
        private static UpdateJobManager<TData> _instance;

        private readonly List<AJobBehaviour<TData>> _dataProviders = new List<AJobBehaviour<TData>>();
        private readonly List<AJobBehaviour<TData>> _dataProvidersToAdd = new List<AJobBehaviour<TData>>();
        private readonly SortedSet<int> _removedDataProviderIndices = new SortedSet<int>();
        private bool _isDirty = true;
        private TransformAccessArray _jobTransforms;
        private NativeArray<TData> _jobData;
        private JobHandle _jobHandle;

        ~UpdateJobManager()
        {
            Dispose();
        }

        public void Process()
        {
            _jobHandle.Complete();

            if (_isDirty)
            {
                RefreshDataProviders();
                FillTransformAccessArray();                
                _isDirty = false;
            }

            if (_dataProviders.Count == 0)
            {
                Dispose();
                return;
            }

            _jobHandle = new UpdateJob<TData>
            {
                Data = _jobData,
            }.Schedule(_jobTransforms);
        }

        public void Dispose()
        {
            _jobHandle.Complete();

            if (_jobData.IsCreated)
            {
                _jobData.Dispose();
            }

            if (_jobTransforms.isCreated)
            {
                _jobTransforms.Dispose();
            }

            UpdateJobRunner.Instance.UnregisterJobManager(this);
        }

        public void AddProvider(AJobBehaviour<TData> provider)
        {
            if (_dataProviders.Count == 0 && _dataProvidersToAdd.Count == 0)
            {
                UpdateJobRunner.Instance.RegisterJobManager(this);
            }
            _dataProvidersToAdd.Add(provider);
            _isDirty = true;
        }

        public void RemoveProvider(AJobBehaviour<TData> provider)
        {
            int index = _dataProviders.BinarySearch(provider, ObjectComparer.Instance);
            if (index >= 0)
            {
                _removedDataProviderIndices.Add(index);
                _isDirty = true;
            }
        }

        public TData GetData(AJobBehaviour<TData> provider)
        {
            int index = _dataProviders.BinarySearch(provider, ObjectComparer.Instance);
            return index >= 0 ? _jobData[index] : default;
        }

        private void FillTransformAccessArray()
        {
            if (_jobTransforms.isCreated)
            {
                _jobTransforms.Dispose();
            }

            TransformAccessArray.Allocate(0, -1, out _jobTransforms);
            for (int i = 0; i < _dataProviders.Count; i++)
            {
                _jobTransforms.Add(_dataProviders[i].transform);
            }
        }

        private void RefreshDataProviders()
        {
#if UNITY_2021_1_OR_NEWER
            using var _ = UnityEngine.Pool.ListPool<TData>.Get(out List<TData> newData);
            newData.AddRange(_jobData);
#else
            List<TData> newData = new List<TData>(_jobData);
#endif
            foreach (int index in _removedDataProviderIndices.Reverse())
            {
                _dataProviders.RemoveAt(index);
                newData.RemoveAt(index);
            }
            _removedDataProviderIndices.Clear();

            foreach (AJobBehaviour<TData> provider in _dataProvidersToAdd)
            {
                int index = _dataProviders.AddSorted(provider, ObjectComparer.Instance);
                if (index >= 0)
                {
                    newData.Insert(index, provider.InitialJobData);
                }
            }
            _dataProvidersToAdd.Clear();

            if (_jobData.IsCreated)
            {
                _jobData.Dispose();
            }
            _jobData = new NativeArray<TData>(newData.Count, Allocator.Persistent);
            for (int i = 0; i < newData.Count; i++)
            {
                _jobData[i] = newData[i];
            }
        }
    }
}
