using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace KSGFK.Unsafe
{
    public unsafe struct NativeStack<T> : IDisposable, IReadOnlyCollection<T> where T : struct
    {
        private void* _data;
        private readonly int _size;
        private int _capacity;
        private int _count;
        private int _allocator;

        public bool IsDisposed => _data == null;

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

        public NativeStack(int size, int allocator, int initCount = 8)
        {
            if (initCount < 0) throw new IndexOutOfRangeException();
            _size = size;
            _allocator = allocator;
            _data = Unsafe.Malloc((ulong) _size * (ulong) initCount, allocator);
            _capacity = initCount;
            _count = 0;
        }

        public NativeStack(int allocator, int initCount = 8) : this(Unsafe.SizeOf<T>(), allocator, initCount) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(T item)
        {
            var last = _count;
            _count++;
            ReSize();
            Unsafe.SetArrayItem(_data, _size, last, ref item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReSize()
        {
            if (_count <= _capacity) return;
            _capacity = Math.Max(_capacity + 1, (int) (_capacity * 1.5f));
            var newPtr = Unsafe.Malloc((ulong) _size * (ulong) _capacity, _allocator);
            Unsafe.CopyData(_data, 0, newPtr, 0, _count, _size);
            Unsafe.Free(_data, _allocator);
            _data = newPtr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Peek()
        {
            if (_count <= 0) throw new IndexOutOfRangeException();
            return ref Unsafe.GetArrayItem<T>(_data, _size, _count - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pop()
        {
            if (_count <= 0) throw new IndexOutOfRangeException();
            _count--;
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

        public void Clear() { _count = 0; }

        public void TrimExcess()
        {
            var newCapacity = (int) (_capacity * 0.9f);
            if (_count < newCapacity)
            {
                _capacity = _count;
                _data = Unsafe.ReAlloc(_data, (ulong) _count, _allocator);
            }
        }

        public Enumerator GetEnumerator() { return new Enumerator(ref this); }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() { return GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly NativeStack<T> _stack;
            private int _idx;

            public ref T Current
            {
                get
                {
                    if (_idx == -2 || _idx == -1) throw new InvalidOperationException();
                    return ref Unsafe.GetArrayItem<T>(_stack._data, _stack._size, _idx);
                }
            }

            T IEnumerator<T>.Current => Current;
            object IEnumerator.Current => Current;

            public Enumerator(ref NativeStack<T> stack)
            {
                _stack = stack;
                _idx = -2;
            }

            public bool MoveNext()
            {
                if (_idx == -2)
                {
                    _idx = _stack._count;
                }

                return --_idx >= 0;
            }

            public void Reset() { _idx = -2; }

            public void Dispose() { _idx = -1; }
        }
    }
}