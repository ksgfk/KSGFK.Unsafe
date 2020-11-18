using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace KSGFK.Unsafe
{
    public unsafe struct NativeList<T> : IDisposable, IList<T> where T : struct
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

        public bool IsReadOnly => false;
        public bool IsDisposed => _data == null;

        public void* Ptr
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data;
        }

        T IList<T>.this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index < 0 || index >= Count) throw new IndexOutOfRangeException($"index:{index} count:{Count}");
                return Unsafe.GetArrayItem<T>(_data, _size, index);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (index < 0 || index >= Count) throw new IndexOutOfRangeException($"index:{index} count:{Count}");
                Unsafe.SetArrayItem(_data, _size, index, ref value);
            }
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index < 0 || index >= Count) throw new IndexOutOfRangeException();
                return ref Unsafe.GetArrayItem<T>(_data, _size, index);
            }
        }

        public NativeList(int initCount, int allocator)
        {
            if (initCount < 0) throw new IndexOutOfRangeException();
            _size = Unsafe.SizeOf<T>();
            _allocator = allocator;
            _data = Unsafe.Malloc((ulong) _size * (ulong) initCount, allocator);
            _capacity = initCount;
            _count = 0;
        }

        private NativeList(void* data, int size, int count, int capacity, int allocator)
        {
            _data = data;
            _size = size;
            _count = count;
            _capacity = capacity;
            _allocator = allocator;
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

        public NativeListEnumerator<T> GetEnumerator() { return new NativeListEnumerator<T>(ref this); }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() { return GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public void Add(T item)
        {
            var last = _count;
            _count++;
            ReSize();
            Unsafe.SetArrayItem(_data, _size, last, ref item);
        }

        public void Add(ref T item)
        {
            var last = _count;
            _count++;
            ReSize();
            Unsafe.SetArrayItem(_data, _size, last, ref item);
        }

        public void Clear() { _count = 0; }

        public bool Contains(T item) { return IndexOf(item) != -1; }

        public void CopyTo(T[] array, int arrayIndex) { Unsafe.CopyData(_data, 0, array, arrayIndex, _count, _size); }

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            RemoveAt(index);
            return true;
        }

        public int IndexOf(T item) { return Unsafe.GetItemIndex(_data, _size, _count, in item); }

        public void Insert(int index, T item)
        {
            if (index < 0 || index >= _count) throw new IndexOutOfRangeException();
            _count++;
            ReSize();
            Unsafe.CopyData(_data, index, _data, index + 1, _count - index, _size);
            Unsafe.SetArrayItem(_data, index, _size, ref item);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count) throw new IndexOutOfRangeException($"index:{index} count:{Count}");
            UnsafeRemoveRange(index, index + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UnsafeRemoveRange(int begin, int end)
        {
            var itemsToRemove = end - begin;
            if (itemsToRemove > 0)
            {
                var copyFrom = Math.Min(begin + itemsToRemove, Count);
                var len = Count - copyFrom;
                Unsafe.CopyData(_data, copyFrom, _data, begin, len, _size);
                _count -= itemsToRemove;
            }
        }

        public void RemoveRange(int begin, int end)
        {
            if (begin < 0 || begin >= Count) throw new IndexOutOfRangeException($"index:{begin} count:{Count}");
            if (end < 0 || end >= Count) throw new IndexOutOfRangeException($"index:{end} count:{Count}");
            if (end < begin) throw new ArgumentException();
            UnsafeRemoveRange(begin, end);
        }

        public void Move(ref NativeList<T> target)
        {
            if (IsDisposed) throw new InvalidOperationException("array is disposed");
            target = new NativeList<T>(_data, _size, _count, _capacity, _allocator);
            _data = null;
            Dispose();
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

        public void Sort() { Unsafe.QuickSort(_data, 0, _count - 1, _size, Comparer<int>.Default); }

        public void Sort(IComparer<T> cmp) { Unsafe.QuickSort(_data, 0, _count - 1, _size, cmp); }
    }

    public struct NativeListEnumerator<T> : IEnumerator<T> where T : struct
    {
        private readonly NativeList<T> _arr;
        private int _i;

        public NativeListEnumerator(ref NativeList<T> arr)
        {
            _arr = arr;
            _i = -1;
        }

        public bool MoveNext() { return ++_i < _arr.Count; }

        public void Reset() { _i = -1; }

        public ref T Current => ref _arr[_i];

        T IEnumerator<T>.Current => _arr[_i];

        object IEnumerator.Current => Current;

        public void Dispose() { }
    }
}