using System;
using Unity.Collections;
using UnityEngine;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public class UpdateJobData<TData, TDataProvider> : IDisposable
        where TData : struct
        where TDataProvider : IInitialJobDataProvider<TData>
    {
        public UnsafeNativeList<TData> Data => _data;
        public ref UnsafeNativeList<TData> DataRef => ref _data;
        public UnsafeNativeList<TData> Backup => _backup;
        public int Length => _data.Length;
        public TData this[int index] => _backup[index];

        protected UnsafeNativeList<TData> _data = new UnsafeNativeList<TData>(Allocator.Persistent);
        protected UnsafeNativeList<TData> _backup = new UnsafeNativeList<TData>(Allocator.Persistent);

        public virtual void EnsureCapacity(int newSize)
        {
            _data.EnsureCapacity(newSize);
        }

        public virtual void Add(TDataProvider dataProvider, int index)
        {
            Debug.Assert(_data.Length == index, "[UpdateJobData] FIXME: added index doesn't match data size");
            _data.Add(dataProvider.InitialJobData);
        }

        public virtual void RemoveAtSwapBack(int index)
        {
            _data.RemoveAtSwapBack(index);
        }

        public virtual void Dispose()
        {
            _data.Dispose();
            _backup.Dispose();
        }

        public void BackupData()
        {
            _backup.CopyFrom(_data);
        }

        public void TrimExcess()
        {
            _data.TrimExcess();
            _backup.TrimExcess();
        }
    }
}
