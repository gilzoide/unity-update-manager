using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public unsafe struct UnsafeNativeList<T> : IDisposable where T : struct
    {
        public static readonly long SizeOfT = UnsafeUtility.SizeOf<T>();
        public static readonly int AlignOfT = UnsafeUtility.AlignOf<T>();

        [NativeDisableUnsafePtrRestriction]
        private void* _buffer;
        private int _length;
        private int _capacity;
        private Allocator Allocator;

        public int Length => _length;
        public long BufferLength => _length * SizeOfT;
        public int Capacity => _capacity;

        public T this[int index]
        {
            get => UnsafeUtility.ReadArrayElement<T>(_buffer, index);
            set => UnsafeUtility.WriteArrayElement<T>(_buffer, index, value);
        }

        public UnsafeNativeList(Allocator allocator)
        {
            _buffer = null;
            _capacity = 0;
            _length = 0;
            Allocator = allocator;
        }

        public void Dispose()
        {
            if (_buffer != null)
            {
                UnsafeUtility.Free(_buffer, Allocator);
                _buffer = null;
            }
            _capacity = 0;
            _length = 0;
        }

        public void EnsureCapacity(int capacity, bool keepData = true)
        {
            if (_capacity < capacity)
            {
                Realloc(capacity, keepData);
            }
        }

        public void Realloc(int newCapacity, bool keepData = true)
        {
            if (newCapacity == _capacity)
            {
                return;
            }
            if (newCapacity == 0)
            {
                Dispose();
                return;
            }

            void* newBuffer = UnsafeUtility.Malloc(newCapacity * SizeOfT, AlignOfT, Allocator);
            if (_buffer != null)
            {
                if (keepData)
                {
                    UnsafeUtility.MemCpy(newBuffer, _buffer, Mathf.Min(_capacity, newCapacity) * SizeOfT);
                }
                UnsafeUtility.Free(_buffer, Allocator);
            }
            _buffer = newBuffer;
            _capacity = newCapacity;
        }

        public void Add(T value)
        {
            _length++;
            ItemRefAt(_length - 1) = value;
        }

        public void RemoveAtSwapBack(int index)
        {
            int lastIndex = _length - 1;
            if (lastIndex > 0 && lastIndex != index)
            {
                ItemRefAt(index) = ItemRefAt(lastIndex);
            }
            _length--;
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
            EnsureCapacity(other._length, false);
            UnsafeUtility.MemCpy(_buffer, other._buffer, other.BufferLength);
        }
    }
}
