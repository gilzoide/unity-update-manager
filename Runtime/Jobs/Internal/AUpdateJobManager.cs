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
        protected readonly ReversedSortedList<int> _dataProvidersToRemove = new ReversedSortedList<int>();
        protected readonly HashSet<IJobDataSynchronizer<TData>> _dataProvidersToSyncEveryFrame = new HashSet<IJobDataSynchronizer<TData>>();
        protected readonly HashSet<IJobDataSynchronizer<TData>> _dataProvidersToSyncOnce = new HashSet<IJobDataSynchronizer<TData>>();
        protected readonly List<IJobDataSynchronizer<TData>> _hashSetFrozenIterator = new List<IJobDataSynchronizer<TData>>();
        protected readonly TJobData _jobData = new TJobData();
        protected JobHandle _jobHandle;

        protected abstract JobHandle ScheduleJob(JobHandle dependsOn);

#if HAVE_BURST
        protected static readonly bool IsJobBurstCompiled = UpdateJobOptions.GetIsBurstCompiled<TData>();
#endif

        private readonly IJobManager[] _dependencyManagers;
        private NativeArray<JobHandle> _dependencyJobHandles;
        private int _lastProcessedFrame;
        private int _dependenciesScheduledCount;
        private bool _isPendingUpdate;

        public AUpdateJobManager()
        {
            _dependencyManagers = UpdateJobOptions.GetDependsOnManagers<TData>();
            Application.quitting += Dispose;
        }

        ~AUpdateJobManager()
        {
            Application.quitting -= Dispose;
            Dispose();
        }

        public void ManagedUpdate()
        {
            _jobHandle.Complete();
            SynchronizeJobData();

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
            Register(provider, false);
        }

        /// <summary>
        /// Register <paramref name="provider"/> to have its update job scheduled every frame
        /// and optionally synchronize job data every frame.
        /// </summary>
        /// <param name="provider">Job data provider</param>
        /// <param name="syncEveryFrame">
        ///   If true and <paramref name="provider"/> implements <see cref="IJobDataSynchronizer{}"/>,
        ///   data synchronization will be called every frame.
        /// </param>
        /// <remarks>
        /// Registering job provider objects is O(1).
        /// Registering an object more than once is a no-op.
        /// </remarks>
        public void Register(TDataProvider provider, bool syncEveryFrame)
        {
            if (syncEveryFrame && provider is IJobDataSynchronizer<TData> jobDataSynchronizer)
            {
                _dataProvidersToSyncEveryFrame.Add(jobDataSynchronizer);
                _dataProvidersToSyncOnce.Remove(jobDataSynchronizer);
            }
            // Avoid adding provider to pending addition list if it is already registered AND is not pending removal
            // Providers marked for unregistration should be removed and re-registered to reset their job data
            if (_providerIndexMap.TryGetValue(provider, out int index) && !_dataProvidersToRemove.Contains(index))
            {
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
            UnregisterSynchronization(provider);
        }

        /// <summary>
        /// Unregister <paramref name="provider"/> from job data synchronization.
        /// </summary>
        /// <remarks>
        /// Unregistering job provider objects is O(1).
        /// Unregistering an object that wasn't registered is a no-op.
        /// </remarks>
        public void UnregisterSynchronization(TDataProvider provider)
        {
            if (provider is IJobDataSynchronizer<TData> jobDataSynchronizer)
            {
                _dataProvidersToSyncEveryFrame.Remove(jobDataSynchronizer);
                _dataProvidersToSyncOnce.Remove(jobDataSynchronizer);
            }
        }

        /// <summary>Check whether <paramref name="provider"/> is registered or pending registration in manager.</summary>
        public bool IsRegistered(TDataProvider provider)
        {
            return _dataProvidersToAdd.Contains(provider)
                || (_providerIndexMap.TryGetValue(provider, out int index) && !_dataProvidersToRemove.Contains(index));
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
        /// Schedules <paramref name="provider"/> to synchronize its own job data only once, in the next processed frame.
        /// </summary>
        /// <remarks>
        /// If <paramref name="provider"/> is not registered for updates, does not implement <see cref="IJobDataSynchronizer{}"/>
        /// or is already registered for data synchronization, this method is a no-op.
        /// </remarks>
        public void SynchronizeJobDataOnce(TDataProvider provider)
        {
            if (IsRegistered(provider)
                && provider is IJobDataSynchronizer<TData> jobDataSynchronizer
                && !_dataProvidersToSyncEveryFrame.Contains(jobDataSynchronizer))
            {
                _dataProvidersToSyncOnce.Add(jobDataSynchronizer);
            }
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
            _dataProvidersToSyncEveryFrame.Clear();
            _dataProvidersToSyncOnce.Clear();

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

            foreach (int indexBeingRemoved in _dataProvidersToRemove)
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
                int index = _dataProviders.Count;
                _jobData.Add(provider, index);
                _providerIndexMap[provider] = index;
                _dataProviders.Add(provider);
            }
            _dataProvidersToAdd.Clear();
        }

        private void SynchronizeJobData()
        {
            SynchronizeJobData(_dataProvidersToSyncEveryFrame);
            SynchronizeJobData(_dataProvidersToSyncOnce);
            _dataProvidersToSyncOnce.Clear();
        }

        private void SynchronizeJobData(HashSet<IJobDataSynchronizer<TData>> synchronizers)
        {
            if (synchronizers.Count == 0)
            {
                return;
            }
            
            _hashSetFrozenIterator.AddRange(synchronizers);
            foreach (IJobDataSynchronizer<TData> jobDataSynchronizer in _hashSetFrozenIterator)
            {
                Debug.Assert(jobDataSynchronizer is TDataProvider, "[UpdateJobManager] FIXME: job data synchronizer should be of a job provider type");
                if (_providerIndexMap.TryGetValue((TDataProvider) jobDataSynchronizer, out int index))
                {
                    try
                    {
                        jobDataSynchronizer.SyncJobData(ref _jobData.DataRef.ItemRefAt(index));
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
            _hashSetFrozenIterator.Clear();
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
