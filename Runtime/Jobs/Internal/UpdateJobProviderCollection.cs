using System;
using Unity.Collections;
using Unity.Jobs;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public class UpdateJobProviderCollection<TData> : JobProviderCollection<IJobUpdatable<TData>>, IDisposable
        where TData : struct, IUpdateJob
    {
        protected NativeArray<TData> _jobData;

        public TData GetData(IJobUpdatable<TData> provider)
        {
            return TryGetIndex(provider, out int index)
                ? _jobData[index]
                : default;
        }

        public void Dispose()
        {
            NativeArrayExtensions.DisposeIfCreated(ref _jobData);
        }

        public JobHandle ScheduleJob(int batchSize)
        {
            return new UpdateJob<TData>
            {
                Data = _jobData,
            }.Schedule(_jobData.Length, batchSize);
        }

        protected override void Add(IJobUpdatable<TData> provider)
        {
            base.Add(provider);
            _jobData[Count - 1] = provider.InitialJobData;
        }

        protected override void RemoveAtSwapBack(int index)
        {
            base.RemoveAtSwapBack(index);
            _jobData.SwapBack(index);
        }

        protected override void RefreshProviders()
        {
            RefreshRemoveProviders();

            int newDataSize = _dataProviders.Count + _dataProvidersToAdd.Count;
            NativeArrayExtensions.Realloc(ref _jobData, newDataSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            RefreshAddProviders();
        }
    }
}
