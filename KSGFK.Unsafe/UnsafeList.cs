using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace KSGFK.Unsafe
{
    public unsafe struct UnsafeList : IDisposable
    {
        private void* _data;
        private int _size;
        private int _count;
        private int _capacity;
        private int _allocator;

        public void* Ptr
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data;
        }

        public int SizeOfItem
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _size;
        }

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

        public UnsafeList(int size, int capacity, int allocator)
        {
            _size = size;
            _capacity = capacity;
            _allocator = allocator;
            _data = Unsafe.Malloc((ulong) _capacity * (ulong) size, allocator);
            _count = 0;
        }

        public UnsafeList(int size, int allocator, int initCount, void* initValue) : this(size, initCount, allocator)
        {
            for (var i = 0; i < initCount; i++)
            {
                Unsafe.SetArrayItem(_data, size, i, initValue);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(T item) where T : struct
        {
            var last = _count;
            _count++;
            ReSize();
            Unsafe.SetArrayItem(_data, _size, last, ref item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(ref T item) where T : struct
        {
            var last = _count;
            _count++;
            ReSize();
            Unsafe.SetArrayItem(_data, _size, last, ref item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void* Get(int index) { return (byte*) _data + index * _size; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>(int index) where T : struct { return ref Unsafe.GetArrayItem<T>(_data, _size, index); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index) { UnsafeRemoveRange(index, index + 1); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> GetSpan<T>() { return new Span<T>(_data, _capacity); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UnsafeRemoveRange(int begin, int end)
        {
            var itemsToRemove = end - begin;
            if (itemsToRemove > 0)
            {
                var copyFrom = Math.Min(begin + itemsToRemove, _count);
                var len = _count - copyFrom;
                Unsafe.CopyData(_data, copyFrom, _data, begin, len, _size);
                _count -= itemsToRemove;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReSize()
        {
            if (_count <= _capacity) return;
            _capacity = Math.Max(_capacity + 1, (int) (_capacity * 1.5f));
            var newPtr = Unsafe.Malloc((ulong) _capacity * (ulong) _size, _allocator);
            Unsafe.CopyData(_data, 0, newPtr, 0, _count, _size);
            Unsafe.Free(_data, _allocator);
            _data = newPtr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReSize(int newCapacity, bool modifyCount = false)
        {
            if (newCapacity > _capacity)
            {
                _capacity = newCapacity;
                var newPtr = Unsafe.Malloc((ulong) _capacity * (ulong) _size, _allocator);
                Unsafe.CopyData(_data, 0, newPtr, 0, _count, _size);
                Unsafe.Free(_data, _allocator);
                _data = newPtr;
            }

            if (modifyCount)
            {
                _count = _capacity;
            }
        }

        public void Dispose()
        {
            Unsafe.Free(_data, _allocator);
            _data = null;
            _count = 0;
            _capacity = 0;
            _allocator = -1;
            _size = 0;
        }

        public UnsafeListEnumerator GetEnumerator() { return new UnsafeListEnumerator(ref this); }
    }

    public unsafe struct UnsafeListEnumerator : IEnumerator<ulong>
    {
        private readonly UnsafeList _ul;
        private int _ind;

        public UnsafeListEnumerator(ref UnsafeList ul)
        {
            _ul = ul;
            _ind = -1;
        }

        public bool MoveNext() { return ++_ind < _ul.Count; }

        public void Reset() { _ind = -1; }

        public ulong Current => (ulong) _ul.Get(_ind);

        object IEnumerator.Current => Current;

        public void Dispose() { }
    }
}