using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace KSGFK.Unsafe
{
    public unsafe struct NativeArray<T> : IDisposable, IList<T> where T : struct
    {
        private void* _data;
        private readonly int _size;
        private int _count;
        private int _allocator;

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
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
                if (index < 0 || index >= _count) throw new IndexOutOfRangeException($"index:{index} count:{Count}");
                return Unsafe.GetArrayItem<T>(_data, _size, index);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (index < 0 || index >= _count) throw new IndexOutOfRangeException($"index:{index} count:{Count}");
                Unsafe.SetArrayItem(_data, _size, index, ref value);
            }
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index < 0 || index >= _count) throw new IndexOutOfRangeException();
                return ref Unsafe.GetArrayItem<T>(_data, _size, index);
            }
        }

        public NativeArray(int count, int allocator)
        {
            if (count <= 0) throw new ArgumentException();
            _size = Unsafe.SizeOf<T>();
            _allocator = allocator;
            _data = Unsafe.Malloc((ulong) _size * (ulong) count, allocator);
            _count = count;
        }

        private NativeArray(void* data, int size, int count, int allocator)
        {
            _data = data;
            _size = size;
            _count = count;
            _allocator = allocator;
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Unsafe.Free(_data, _allocator);
                _data = null;
                _count = 0;
                _allocator = -1;
            }
        }

        public NativeArrayEnumerator<T> GetEnumerator() { return new NativeArrayEnumerator<T>(ref this); }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() { return GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        void ICollection<T>.Add(T item) { throw new NotSupportedException(); }

        public void Clear() { Unsafe.Clear(_data, _count); }

        public bool Contains(T item) { return IndexOf(item) != -1; }

        public void CopyTo(T[] array, int arrayIndex) { Unsafe.CopyData(_data, 0, array, arrayIndex, _count, _size); }

        bool ICollection<T>.Remove(T item) { throw new NotSupportedException(); }

        public int IndexOf(T item) { return Unsafe.GetItemIndex(_data, _size, _count, in item); }

        void IList<T>.Insert(int index, T item) { throw new NotSupportedException(); }

        void IList<T>.RemoveAt(int index) { throw new NotSupportedException(); }

        public void Move(ref NativeArray<T> target)
        {
            if (IsDisposed) throw new InvalidOperationException("array is disposed");
            target = new NativeArray<T>(_data, _size, _count, _allocator);
            _data = null;
            Dispose();
        }

        public void Sort() { Unsafe.QuickSort(_data, 0, _count - 1, _size, Comparer<int>.Default); }

        public void Sort(IComparer<T> cmp) { Unsafe.QuickSort(_data, 0, _count - 1, _size, cmp); }
    }

    public struct NativeArrayEnumerator<T> : IEnumerator<T> where T : struct
    {
        private readonly NativeArray<T> _arr;
        private int _i;

        public NativeArrayEnumerator(ref NativeArray<T> arr)
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