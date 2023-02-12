using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public class UpdateTransformJobProviderCollection<TData> : JobProviderCollection<ITransformJobUpdatable<TData>>, IDisposable
        where TData : struct, IUpdateTransformJob
    {
        protected NativeArray<TData> _jobData;
        protected TransformAccessArray _jobTransforms;

        public TData GetData(ITransformJobUpdatable<TData> provider)
        {
            return TryGetIndex(provider, out int index)
                ? _jobData[index]
                : default;
        }

        public void Dispose()
        {
            NativeArrayExtensions.DisposeIfCreated(ref _jobData);
            TransformAccessArrayExtensions.DisposeIfCreated(ref _jobTransforms);
        }

        public JobHandle ScheduleJob()
        {
            return new UpdateTransformJob<TData>
            {
                Data = _jobData,
            }.Schedule(_jobTransforms);
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
            NativeArrayExtensions.Realloc(ref _jobData, newDataSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            RefreshAddProviders();
        }
    }
}
