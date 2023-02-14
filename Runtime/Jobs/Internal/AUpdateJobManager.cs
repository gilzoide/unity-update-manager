using System;
using System.Collections.Generic;
using Unity.Jobs;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public abstract class AUpdateJobManager<TData, TDataProvider, TJobData> : IUpdatable, IDisposable
        where TData : struct
        where TDataProvider : IInitialJobDataProvider<TData>
        where TJobData : UpdateJobData<TData, TDataProvider>, new()
    {
        protected readonly Dictionary<TDataProvider, int> _providerIndexMap = new Dictionary<TDataProvider, int>();
        protected readonly List<TDataProvider> _dataProviders = new List<TDataProvider>();
        protected readonly List<TDataProvider> _dataProvidersToAdd = new List<TDataProvider>();
        protected readonly SortedSet<int> _dataProvidersToRemove = new SortedSet<int>();
        protected readonly TJobData _jobData = new TJobData();
        protected bool _isDirty = true;
        protected JobHandle _jobHandle;

        protected abstract JobHandle ScheduleJob();

        ~AUpdateJobManager()
        {
            Dispose();
        }

        public void ManagedUpdate()
        {
            _jobHandle.Complete();

            if (_isDirty)
            {
                RefreshProviders();
                _isDirty = false;
            }

            if (_dataProviders.Count == 0)
            {
                Dispose();
                return;
            }

            _jobData.BackupData();
            _jobHandle = ScheduleJob();
        }

        public void Register(TDataProvider provider)
        {
            if (_providerIndexMap.ContainsKey(provider))
            {
                return;
            }

            if (_dataProviders.Count == 0 && _dataProvidersToAdd.Count == 0)
            {
                UpdateJobTime.Instance.RegisterUpdate();
                UpdateManager.Instance.Register(this);
            }

            _dataProvidersToAdd.Add(provider);
            _isDirty = true;
        }

        public void Unregister(TDataProvider provider)
        {
            if (_providerIndexMap.TryGetValue(provider, out int index))
            {
                _dataProvidersToRemove.Add(index);
                _isDirty = true;
            }
        }

        public TData GetData(TDataProvider provider)
        {
            return _providerIndexMap.TryGetValue(provider, out int index)
                ? _jobData[index]
                : default;
        }

        public void Dispose()
        {
            _jobHandle.Complete();

            _jobData.Dispose();

            _providerIndexMap.Clear();
            _dataProviders.Clear();
            _dataProvidersToAdd.Clear();
            _dataProvidersToRemove.Clear();
            _isDirty = false;

            UpdateJobTime.Instance.UnregisterUpdate();
            UpdateManager.Instance.Unregister(this);
        }

        protected void RefreshProviders()
        {
            RemovePendingProviders();

            int newDataSize = _dataProviders.Count + _dataProvidersToAdd.Count;
            _jobData.EnsureCapacity(newDataSize);

            AddPendingProviders();
        }

        protected void RemovePendingProviders()
        {
            if (_dataProvidersToRemove.Count == 0)
            {
                return;
            }

            foreach (int indexBeingRemoved in _dataProvidersToRemove.Reverse())
            {
                _providerIndexMap.Remove(_dataProviders[indexBeingRemoved]);

                _dataProviders.RemoveAtSwapBack(indexBeingRemoved, out TDataProvider swappedProvider);
                if (swappedProvider != null)
                {
                    _providerIndexMap[swappedProvider] = indexBeingRemoved;
                }

                _jobData.RemoveAtSwapBack(indexBeingRemoved);
            }
            _dataProvidersToRemove.Clear();
        }

        protected void AddPendingProviders()
        {
            if (_dataProvidersToAdd.Count == 0)
            {
                return;
            }

            foreach (TDataProvider provider in _dataProvidersToAdd)
            {
                _dataProviders.Add(provider);
                int index = _dataProviders.Count - 1;
                _providerIndexMap[provider] = index;
                _jobData.Add(provider, index);
            }
            _dataProvidersToAdd.Clear();
        }
    }
}
