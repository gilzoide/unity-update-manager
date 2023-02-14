using System;
using Unity.Collections;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public class JobDoubleBuffer<TData> : IDisposable
        where TData : struct
    {
        public UnsafeNativeList<TData> Data => _data;
        public UnsafeNativeList<TData> Backup => _backup;
        public int Length => _data.Length;

        public TData this[int index]
        {
            get => _backup[index];
            set => _data[index] = value;
        }

        protected UnsafeNativeList<TData> _data = new UnsafeNativeList<TData>(Allocator.Persistent);
        protected UnsafeNativeList<TData> _backup = new UnsafeNativeList<TData>(Allocator.Persistent);
        protected bool _array1IsActive;

        public void Resize(int newSize)
        {
            _data.Realloc(newSize);
        }

        public void Dispose()
        {
            _data.Dispose();
            _backup.Dispose();
        }

        public void SwapBack(int index)
        {
            _data.SwapBack(index);
        }

        public void BackupData()
        {
            _backup.CopyFrom(_data);
        }
    }
}
