using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public unsafe struct UnsafeNativeList<T> : IDisposable where T : struct
    {
        public static readonly int SizeOfT = UnsafeUtility.SizeOf<T>();
        public static readonly int AlignOfT = UnsafeUtility.AlignOf<T>();

        [NativeDisableUnsafePtrRestriction]
        private void* _buffer;
        private int _length;
        private Allocator Allocator;

        public int Length => _length;
        public int BufferLength => _length * SizeOfT;

        public T this[int index]
        {
            get => UnsafeUtility.ReadArrayElement<T>(_buffer, index);
            set => UnsafeUtility.WriteArrayElement<T>(_buffer, index, value);
        }

        public UnsafeNativeList(Allocator allocator)
        {
            _buffer = null;
            _length = 0;
            Allocator = allocator;
        }

        public UnsafeNativeList(int length, Allocator allocator)
        {
            _buffer = null;
            _length = 0;
            Allocator = allocator;
            Realloc(length, false);
        }

        public void Dispose()
        {
            if (_buffer != null)
            {
                UnsafeUtility.Free(_buffer, Allocator);
                _buffer = null;
                _length = 0;
            }
        }

        public void Realloc(int newLength, bool keepData = true)
        {
            if (newLength == _length)
            {
                return;
            }
            if (newLength == 0)
            {
                Dispose();
                return;
            }

            void* newBuffer = UnsafeUtility.Malloc(newLength * SizeOfT, AlignOfT, Allocator);
            if (_buffer != null)
            {
                if (keepData)
                {
                    UnsafeUtility.MemCpy(newBuffer, _buffer, Mathf.Min(_length, newLength) * SizeOfT);
                }
                Dispose();
            }
            _buffer = newBuffer;
            _length = newLength;
        }

        public void SwapBack(int index)
        {
            int lastIndex = _length - 1;
            if (lastIndex > 0 && lastIndex != index)
            {
                ItemRefAt(index) = ItemRefAt(_length - 1);
            }
        }

        public ref T ItemRefAt(int index)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (index < 0 || index >= _length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (_buffer == null)
            {
                throw new NullReferenceException("Buffer is null");
            }
#endif
            return ref UnsafeUtility.ArrayElementAsRef<T>(_buffer, index);
        }

        public void CopyFrom(UnsafeNativeList<T> other)
        {
            Realloc(other._length, false);
            UnsafeUtility.MemCpy(_buffer, other._buffer, BufferLength);
        }
    }
}
