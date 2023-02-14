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
        public void* Buffer;
        public int Length;
        public Allocator Allocator;

        public int BufferLength => Length * SizeOfT;

        public T this[int index]
        {
            get => ItemRefAt(index);
            set => ItemRefAt(index) = value;
        }

        public UnsafeNativeList(Allocator allocator)
        {
            Buffer = null;
            Length = 0;
            Allocator = allocator;
        }

        public UnsafeNativeList(int length, Allocator allocator)
        {
            Buffer = null;
            Length = 0;
            Allocator = allocator;
            Realloc(length, false);
        }

        public void Dispose()
        {
            if (Buffer != null)
            {
                UnsafeUtility.Free(Buffer, Allocator);
                Buffer = null;
            }
        }

        public void Realloc(int newLength, bool keepData = true)
        {
            if (newLength == Length)
            {
                return;
            }
            if (newLength == 0)
            {
                Dispose();
                return;
            }

            void* newBuffer = UnsafeUtility.Malloc(newLength * SizeOfT, AlignOfT, Allocator);
            if (Buffer != null)
            {
                if (keepData)
                {
                    UnsafeUtility.MemCpy(newBuffer, Buffer, Mathf.Min(Length, newLength) * SizeOfT);
                }
                Dispose();
            }
            Buffer = newBuffer;
            Length = newLength;
        }

        public void SwapBack(int index)
        {
            int lastIndex = Length - 1;
            if (lastIndex > 0 && lastIndex != index)
            {
                ItemRefAt(index) = ItemRefAt(Length - 1);
            }
        }

        public ref T ItemRefAt(int index)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (index < 0 || index >= Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
#endif
            return ref UnsafeUtility.ArrayElementAsRef<T>(Buffer, index);
        }

        public void CopyFrom(UnsafeNativeList<T> other)
        {
            Realloc(other.Length, false);
            UnsafeUtility.MemCpy(Buffer, other.Buffer, BufferLength);
        }
    }
}
