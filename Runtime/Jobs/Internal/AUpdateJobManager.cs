using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public abstract class AUpdateJobManager<TData, TDataProvider, TJobData> : IJobManager, IUpdatable, IDisposable
        where TData : struct
        where TDataProvider : IInitialJobDataProvider<TData>
        where TJobData : UpdateJobData<TData, TDataProvider>, new()
    {
        public event Action<JobHandle> OnJobScheduled;

        protected readonly Dictionary<TDataProvider, int> _providerIndexMap = new Dictionary<TDataProvider, int>();
        protected readonly List<TDataProvider> _dataProviders = new List<TDataProvider>();
        protected readonly List<TDataProvider> _dataProvidersToAdd = new List<TDataProvider>();
        protected readonly SortedSet<int> _dataProvidersToRemove = new SortedSet<int>();
        protected readonly TJobData _jobData = new TJobData();
        protected bool _isDirty = true;
        protected JobHandle _jobHandle;

        protected abstract JobHandle ScheduleJob(JobHandle dependsOn);

        private readonly IJobManager[] _dependencyManagers;
        private NativeArray<JobHandle> _dependencyJobHandles;
        private int _lastProcessedFrame;
        private int _dependenciesScheduledCount;
        private bool _isPendingUpdate;

        public AUpdateJobManager()
        {
            _dependencyManagers = UpdateJobOptions.GetDependsOnManagers<TData>();
        }

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

            _isPendingUpdate = false;
            ScheduleJobIfDependenciesMet();
        }

        public void Register(TDataProvider provider)
        {
            if (_providerIndexMap.ContainsKey(provider))
            {
                return;
            }

            if (_dataProviders.Count == 0 && _dataProvidersToAdd.Count == 0)
            {
                StartUpdating();
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

            StopUpdating();
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

        private void StartUpdating()
        {
            UpdateJobTime.Instance.RegisterUpdate();
            UpdateManager.Instance.Register(this);

            for (int i = 0; i < _dependencyManagers.Length; i++)
            {
                _dependencyManagers[i].OnJobScheduled += ScheduleJobIfDependenciesMet;
            }
            _dependencyJobHandles = new NativeArray<JobHandle>(_dependencyManagers.Length, Allocator.Persistent);
        }

        private void StopUpdating()
        {
            UpdateJobTime.Instance.UnregisterUpdate();
            UpdateManager.Instance.Unregister(this);

            for (int i = 0; i < _dependencyManagers.Length; i++)
            {
                _dependencyManagers[i].OnJobScheduled -= ScheduleJobIfDependenciesMet;
            }
            _dependencyJobHandles.Dispose();
        }

        private void ScheduleJobIfDependenciesMet()
        {
            if (!AreAllDependenciesFulfilled())
            {
                return;
            }

            _isPendingUpdate = true;
            _jobHandle = ScheduleJob(JobHandle.CombineDependencies(_dependencyJobHandles));
            OnJobScheduled?.Invoke(_jobHandle);
        }

        private void ScheduleJobIfDependenciesMet(JobHandle dependecyJobHandle)
        {
            MarkDependencyMet();
            _dependencyJobHandles[_dependenciesScheduledCount - 1] = dependecyJobHandle;
            ScheduleJobIfDependenciesMet();
        }

        private bool AreAllDependenciesFulfilled()
        {
            return !_isPendingUpdate && _dependenciesScheduledCount >= _dependencyJobHandles.Length;
        }

        private void MarkDependencyMet()
        {
            int currentFrame = Time.frameCount;
            if (currentFrame != _lastProcessedFrame)
            {
                _dependenciesScheduledCount = 0;
                _lastProcessedFrame = currentFrame;
            }
            _dependenciesScheduledCount++;
        }
    }
}
