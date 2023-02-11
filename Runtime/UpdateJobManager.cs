using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
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

        public UpdateJobManager()
        {
            UpdateJobRunner.Instance.RegisterJobManager(this);
        }

        public void Process()
        {
            CompleteJob();

            if (_isDirty)
            {
                if (_jobTransforms.isCreated)
                {
                    _jobTransforms.Dispose();
                }
                
                RefreshDataProviders();
                FillTransformAccessArray();                
                _isDirty = false;
            }

            FillData();
            _jobHandle = new UpdateJob<TData>
            {
                Data = _jobData,
            }.Schedule(_jobTransforms);
        }

        public void Dispose()
        {
            CompleteJob();

            if (_jobTransforms.isCreated)
            {
                _jobTransforms.Dispose();
            }
        }

        public void AddProvider(AJobBehaviour<TData> provider)
        {
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

        private void CompleteJob()
        {
            _jobHandle.Complete();
            if (_jobData.IsCreated)
            {
                for (int i = 0; i < _jobData.Length; i++)
                {
                    if (!_removedDataProviderIndices.Contains(i))
                    {
                        _dataProviders[i].Data = _jobData[i];
                    }
                }
                _jobData.Dispose();
            }
        }

        private void FillTransformAccessArray()
        {
            TransformAccessArray.Allocate(_dataProviders.Count, -1, out _jobTransforms);
            for (int i = 0; i < _dataProviders.Count; i++)
            {
                _jobTransforms.Add(_dataProviders[i].transform);
            }
        }

        private void RefreshDataProviders()
        {
            foreach (int index in _removedDataProviderIndices.Reverse())
            {
                _dataProviders.RemoveAt(index);
            }
            _removedDataProviderIndices.Clear();

            foreach (AJobBehaviour<TData> provider in _dataProvidersToAdd)
            {
                int index = _dataProviders.BinarySearch(provider, ObjectComparer.Instance);
                if (index < 0)
                {
                    _dataProviders.Insert(~index, provider);
                }
            }
            _dataProvidersToAdd.Clear();
        }

        private void FillData()
        {
            _jobData = new NativeArray<TData>(_dataProviders.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < _dataProviders.Count; i++)
            {
                _jobData[i] = _dataProviders[i].Data;
            }
        }
    }
}
