using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager.Jobs
{
    public class UpdateJobManager<TData> : IUpdatable
        where TData : struct, IUpdateJob
    {
        public static UpdateJobManager<TData> Instance => _instance != null ? _instance : (_instance = new UpdateJobManager<TData>());
        private static UpdateJobManager<TData> _instance;

        private readonly Dictionary<AJobBehaviour<TData>, int> _providerIndexMap = new Dictionary<AJobBehaviour<TData>, int>();
        private readonly List<AJobBehaviour<TData>> _dataProviders = new List<AJobBehaviour<TData>>();
        private readonly List<AJobBehaviour<TData>> _dataProvidersToAdd = new List<AJobBehaviour<TData>>();
        private readonly List<int> _dataProvidersToRemove = new List<int>();
        private bool _isDirty = true;
        private TransformAccessArray _jobTransforms;
        private NativeArray<TData> _jobData;
        private JobHandle _jobHandle;

        ~UpdateJobManager()
        {
            Dispose();
        }

        public void ManagedUpdate()
        {
            _jobHandle.Complete();

            if (_isDirty)
            {
                RefreshDataProviders();            
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

            NativeArrayExtensions.DisposeIfCreated(ref _jobData);
            TransformAccessArrayExtensions.DisposeIfCreated(ref _jobTransforms);

            UpdateJobTime.Instance.UnregisterUpdate();
            UpdateManager.Instance.UnregisterUpdatable(this);
        }

        public void AddProvider(AJobBehaviour<TData> provider)
        {
            if (_providerIndexMap.ContainsKey(provider))
            {
                return;
            }

            if (_dataProviders.Count == 0 && _dataProvidersToAdd.Count == 0)
            {
                UpdateJobTime.Instance.RegisterUpdate();
                UpdateManager.Instance.RegisterUpdatable(this);
            }
            _dataProvidersToAdd.Add(provider);
            _isDirty = true;
        }

        public void RemoveProvider(AJobBehaviour<TData> provider)
        {
            if (!_providerIndexMap.TryGetValue(provider, out int index))
            {
                _dataProvidersToRemove.Add(index);
                _isDirty = true;
            }
        }

        public TData GetData(AJobBehaviour<TData> provider)
        {
            return _providerIndexMap.TryGetValue(provider, out int index)
                ? _jobData[index]
                : default;
        }

        private void RefreshDataProviders()
        {
            if (!_jobTransforms.isCreated)
            {
                TransformAccessArray.Allocate(_dataProvidersToAdd.Count, -1, out _jobTransforms);
            }

            RefreshRemoveProviders();

            int newDataSize = _dataProviders.Count + _dataProvidersToAdd.Count;
            NativeArrayExtensions.Realloc(ref _jobData, newDataSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            
            RefreshAddProviders();
        }

        private void RefreshRemoveProviders()
        {
            if (_dataProvidersToRemove.Count == 0)
            {
                return;
            }

            foreach (int indexBeingRemoved in _dataProvidersToRemove)
            {
                _providerIndexMap.Remove(_dataProviders[indexBeingRemoved]);

                _dataProviders.RemoveAtSwapBack(indexBeingRemoved, out AJobBehaviour<TData> swappedProvider);
                if (swappedProvider)
                {
                    _providerIndexMap[swappedProvider] = indexBeingRemoved;
                }
                _jobTransforms.RemoveAtSwapBack(indexBeingRemoved);
                _jobData.SwapBack(indexBeingRemoved);
            }
            _dataProvidersToRemove.Clear();
        }

        private void RefreshAddProviders()
        {
            if (_dataProvidersToAdd.Count == 0)
            {
                return;
            }

            foreach (AJobBehaviour<TData> provider in _dataProvidersToAdd)
            {
                _dataProviders.Add(provider);
                int index = _dataProviders.Count - 1;
                _providerIndexMap[provider] = index;
                
                _jobTransforms.Add(provider.transform);
                _jobData[index] = provider.InitialJobData;
            }
            _dataProvidersToAdd.Clear();
        }
    }
}
