using System;
using Unity.Collections;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public class JobDoubleBuffer<TData> : IDisposable
        where TData : struct
    {
        public NativeArray<TData> Data => _data;
        public NativeArray<TData>.ReadOnly Backup => _backup.AsReadOnly();
        public int Length => _data.Length;

        public TData this[int index]
        {
            get => _backup[index];
            set => _data[index] = value;
        }

        protected NativeArray<TData> _data;
        protected NativeArray<TData> _backup;
        protected bool _array1IsActive;

        public void Resize(int newSize)
        {
            NativeArrayExtensions.Realloc(ref _data, newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        public void Dispose()
        {
            NativeArrayExtensions.DisposeIfCreated(ref _data);
            NativeArrayExtensions.DisposeIfCreated(ref _backup);
        }

        public void SwapBack(int index)
        {
            _data.SwapBack(index);
        }

        public void BackupData()
        {
            if (_backup.Length == _data.Length)
            {
                _data.CopyTo(_backup);
            }
            else
            {
                NativeArrayExtensions.DisposeIfCreated(ref _backup);
                _backup = new NativeArray<TData>(_data, Allocator.Persistent);
            }
        }
    }
}
