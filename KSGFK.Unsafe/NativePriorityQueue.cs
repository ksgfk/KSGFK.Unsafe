using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace KSGFK.Unsafe
{
    public unsafe struct NativePriorityQueue<T> : IDisposable where T : struct
    {
        private void* _data;
        private readonly int _size;
        private int _count;
        private int _capacity;
        private int _allocator;

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _capacity;
        }

        public void* Ptr
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data;
        }

        public bool IsDisposed => _data == null;
        public bool IsReadOnly => false;

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count <= 0;
        }

        public NativePriorityQueue(int initCount, int allocator)
        {
            if (initCount < 0) throw new IndexOutOfRangeException();
            _size = Unsafe.SizeOf<T>();
            _allocator = allocator;
            _data = Unsafe.Malloc((ulong) _size * (ulong) initCount, allocator);
            _capacity = initCount;
            _count = 0;
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Unsafe.Free(_data, _allocator);
                _data = null;
                _count = 0;
                _capacity = 0;
                _allocator = -1;
            }
        }

        public void Enqueue(T item)
        {
            _count++;
            ReSize();
            Unsafe.SetArrayItem(_data, _size, _count - 1, ref item);
            Unsafe.PushHeap(_data, (byte*) _data + _count * _size, _size, Comparer<T>.Default);
        }

        public void Enqueue(T item, IComparer<T> cmp)
        {
            _count++;
            ReSize();
            Unsafe.SetArrayItem(_data, _size, _count - 1, ref item);
            Unsafe.PushHeap(_data, (byte*) _data + _count * _size, _size, cmp);
        }

        public void Dequeue()
        {
            var data = (byte*) _data;
            Unsafe.PopHeap(_data, data + _count * _size, _size, Comparer<T>.Default);
            _count--;
        }

        public void Dequeue(IComparer<T> cmp)
        {
            var data = (byte*) _data;
            Unsafe.PopHeap(_data, data + _count * _size, _size, cmp);
            _count--;
        }

        public T Peek()
        {
            if (_count <= 0) throw new IndexOutOfRangeException();
            return Unsafe.GetArrayItem<T>(_data, _size, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReSize()
        {
            if (_count <= _capacity) return;
            _capacity = Math.Max(_capacity + 1, (int) (_capacity * 1.5f));
            var newPtr = Unsafe.Malloc((ulong) _size * (ulong) _capacity, _allocator);
            Unsafe.CopyData(_data, 0, newPtr, 0, _count, _size);
            Unsafe.Free(_data, _allocator);
            _data = newPtr;
        }
    }
}