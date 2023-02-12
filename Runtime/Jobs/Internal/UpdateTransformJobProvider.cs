using System;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public class UpdateTransformJobProvider<TData> : JobProviderCollection<ITransformJobUpdatable<TData>>, IDisposable
        where TData : struct, IUpdateTransformJob
    {
        public static int JobBatchSize = UpdateJobOptions.GetBatchSize<TData>();
        public static bool ReadOnlyTransforms = UpdateJobOptions.GetReadOnlyTransforms<TData>();

        protected readonly JobDoubleBuffer<TData> _jobData = new JobDoubleBuffer<TData>();
        protected TransformAccessArray _jobTransforms;

        public TData GetData(ITransformJobUpdatable<TData> provider)
        {
            return TryGetIndex(provider, out int index)
                ? _jobData[index]
                : default;
        }

        public void Dispose()
        {
            _jobData.Dispose();
            TransformAccessArrayExtensions.DisposeIfCreated(ref _jobTransforms);
        }

        public JobHandle ScheduleJob()
        {
            _jobData.BackupData();
            var job = new UpdateTransformJob<TData>
            {
                Data = _jobData.Data,
            };
            if (ReadOnlyTransforms)
            {
                return job.ScheduleReadOnly(_jobTransforms, JobBatchSize);
            }
            else
            {
                return job.Schedule(_jobTransforms);
            }
        }

        protected override void Add(ITransformJobUpdatable<TData> provider)
        {
            base.Add(provider);
            _jobData[Count - 1] = provider.InitialJobData;
            _jobTransforms.Add(provider.Transform);
        }

        protected override void RemoveAtSwapBack(int index)
        {
            base.RemoveAtSwapBack(index);
            _jobData.SwapBack(index);
            _jobTransforms.RemoveAtSwapBack(index);
        }

        protected override void RefreshProviders()
        {
            if (!_jobTransforms.isCreated)
            {
                TransformAccessArray.Allocate(_dataProvidersToAdd.Count, -1, out _jobTransforms);
            }

            RefreshRemoveProviders();

            int newDataSize = _dataProviders.Count + _dataProvidersToAdd.Count;
            _jobData.Resize(newDataSize);

            RefreshAddProviders();
        }
    }
}
