using System;
using Unity.Jobs;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public class UpdateJobProvider<TData> : JobProviderCollection<IJobUpdatable<TData>>, IDisposable
        where TData : struct, IUpdateJob
    {
        public static int JobBatchSize = UpdateJobOptions.GetBatchSize<TData>();

        protected JobDoubleBuffer<TData> _jobData = new JobDoubleBuffer<TData>();

        public TData GetData(IJobUpdatable<TData> provider)
        {
            return TryGetIndex(provider, out int index)
                ? _jobData[index]
                : default;
        }

        public void Dispose()
        {
            _jobData.Dispose();
        }

        public JobHandle ScheduleJob()
        {
            _jobData.BackupData();
            return new UpdateJob<TData>
            {
                Data = _jobData.Data,
            }.Schedule(_jobData.Length, JobBatchSize);
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
            _jobData.Resize(newDataSize);

            RefreshAddProviders();
        }
    }
}
