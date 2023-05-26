using System;
using System.Collections.Generic;
using Gilzoide.UpdateManager.Extensions;
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
        /// <summary>
        /// Event invoked when the update job is scheduled.
        /// </summary>
        /// <remarks>
        /// This is used internally for implementing dependencies between managed jobs.
        /// </remarks>
        public event Action<JobHandle> OnJobScheduled;

        protected bool HavePendingProviderChanges => _dataProvidersToAdd.Count > 0 || _dataProvidersToRemove.Count > 0;
        protected readonly Dictionary<TDataProvider, int> _providerIndexMap = new Dictionary<TDataProvider, int>();
        protected readonly List<TDataProvider> _dataProviders = new List<TDataProvider>();
        protected readonly HashSet<TDataProvider> _dataProvidersToAdd = new HashSet<TDataProvider>();
        protected readonly SortedSet<int> _dataProvidersToRemove = new SortedSet<int>();
        protected readonly TJobData _jobData = new TJobData();
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
            Application.quitting += Dispose;
            Application.lowMemory += _jobData.TrimExcess;
        }

        ~AUpdateJobManager()
        {
            Application.quitting -= Dispose;
            Application.lowMemory -= _jobData.TrimExcess;
            Dispose();
        }

        public void ManagedUpdate()
        {
            _jobHandle.Complete();

            if (HavePendingProviderChanges)
            {
                RefreshProviders();
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

        /// <summary>
        /// Register <paramref name="provider"/> to have its update job scheduled every frame.
        /// </summary>
        /// <remarks>
        /// Registering job provider objects is O(1).
        /// Registering an object more than once is a no-op.
        /// </remarks>
        public void Register(TDataProvider provider)
        {
            if (_providerIndexMap.TryGetValue(provider, out int index))
            {
                _dataProvidersToRemove.Remove(index);
                return;
            }

            bool addedToPending = _dataProvidersToAdd.Add(provider);
            if (addedToPending && _dataProviders.Count == 0 && _dataProvidersToAdd.Count == 1)
            {
                StartUpdating();
            }
        }

        /// <summary>
        /// Unregister <paramref name="provider"/>, so its update job is not scheduled anymore.
        /// </summary>
        /// <remarks>
        /// Unregistering job provider objects is O(1).
        /// Unregistering an object that wasn't registered is a no-op.
        /// </remarks>
        public void Unregister(TDataProvider provider)
        {
            _dataProvidersToAdd.Remove(provider);
            if (_providerIndexMap.TryGetValue(provider, out int index))
            {
                _dataProvidersToRemove.Add(index);
            }
        }

        /// <summary>
        /// Get a copy of the job data from the last processed frame for <paramref name="provider"/>.
        /// </summary>
        /// <remarks>
        /// Since update job managers use double buffering for the data, you can access a copy of the last processed data at any time, even while jobs are scheduled and/or processing.
        /// <br/>
        /// Calling this with an unregistered provider returns <c>provider.InitialJobData</c>.
        /// </remarks>
        public TData GetData(TDataProvider provider)
        {
            return _providerIndexMap.TryGetValue(provider, out int index)
                ? _jobData[index]
                : provider.InitialJobData;
        }

        /// <summary>
        /// Completes current running job, if there is any, remove all job providers and clears up used memory.
        /// </summary>
        /// <remarks>
        /// This is called automatically when all job providers are unregistered or when the manager is finalized.
        /// You shouldn't need to call this manually at any point.
        /// </remarks>
        public void Dispose()
        {
            _jobHandle.Complete();

            _jobData.Dispose();

            _providerIndexMap.Clear();
            _dataProviders.Clear();
            _dataProvidersToAdd.Clear();
            _dataProvidersToRemove.Clear();

            StopUpdating();
        }

        private void RefreshProviders()
        {
            RemovePendingProviders();

            int newDataSize = _dataProviders.Count + _dataProvidersToAdd.Count;
            _jobData.EnsureCapacity(newDataSize);

            AddPendingProviders();
        }

        private void RemovePendingProviders()
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

        private void AddPendingProviders()
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
            _isPendingUpdate = true;

            _dependencyJobHandles.DisposeIfCreated();
            _dependencyJobHandles = new NativeArray<JobHandle>(_dependencyManagers.Length, Allocator.Persistent);
            for (int i = 0; i < _dependencyManagers.Length; i++)
            {
                _dependencyManagers[i].OnJobScheduled += ScheduleJobIfDependenciesMet;
            }

            UpdateManager.Instance.Register(this);
        }

        private void StopUpdating()
        {
            UpdateManager.Instance.Unregister(this);

            for (int i = 0; i < _dependencyManagers.Length; i++)
            {
                _dependencyManagers[i].OnJobScheduled -= ScheduleJobIfDependenciesMet;
            }
            _dependencyJobHandles.DisposeIfCreated();
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
