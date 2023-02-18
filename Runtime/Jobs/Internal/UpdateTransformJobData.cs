using Gilzoide.UpdateManager.Extensions;
using UnityEngine.Jobs;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public class UpdateTransformJobData<TData, TDataProvider> : UpdateJobData<TData, TDataProvider>
        where TData : struct
        where TDataProvider : IInitialTransformJobDataProvider<TData>
    {
        public TransformAccessArray Transforms => _transforms;

        protected TransformAccessArray _transforms;

        public override void EnsureCapacity(int newSize)
        {
            base.EnsureCapacity(newSize);
            if (!_transforms.isCreated)
            {
                TransformAccessArray.Allocate(newSize, -1, out _transforms);
            }
        }

        public override void Add(TDataProvider dataProvider, int index)
        {
            base.Add(dataProvider, index);
            _transforms.Add(dataProvider.Transform);
        }

        public override void RemoveAtSwapBack(int index)
        {
            base.RemoveAtSwapBack(index);
            _transforms.RemoveAtSwapBack(index);
        }

        public override void Dispose()
        {
            base.Dispose();
            _transforms.DisposeIfCreated();
        }
    }
}
