using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KSGFK.Unsafe
{
    public unsafe struct NativeQueue<T> : IDisposable, IEnumerable<T> where T : struct
    {
        private void* _data;
        private readonly int _size;
        private int _capacity;
        private int _count;
        private int _allocator;

        private int _head;
        private int _tail;

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

        public NativeQueue(int size, int allocator, int initCount = 8)
        {
            if (initCount < 0) throw new IndexOutOfRangeException();
            _size = size;
            _allocator = allocator;
            _data = Unsafe.Malloc((ulong) _size * (ulong) initCount, allocator);
            _capacity = initCount;
            _count = 0;
            _head = 0;
            _tail = 0;
        }

        public NativeQueue(int allocator, int initCount = 8) : this(Unsafe.SizeOf<T>(), allocator, initCount) { }

        public void Enqueue(T item)
        {
            ReSize();
            Unsafe.SetArrayItem(_data, _size, _tail, ref item);
            _count++;
            MoveNext(ref _tail);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReSize()
        {
            if (_count < _capacity) return;
            var newCapacity = Math.Max(_capacity + 4, (int) (_capacity * 1.5f));
            var newPtr = Unsafe.Malloc((ulong) _size * (ulong) newCapacity, _allocator);
            if (_count > 0)
            {
                if (_head < _tail)
                {
                    Unsafe.CopyData(_data, _head, newPtr, 0, _count, _size);
                }
                else
                {
                    Unsafe.CopyData(_data, _head, newPtr, 0, _capacity - _head, _size);
                    Unsafe.CopyData(_data, 0, newPtr, _capacity - _head, _tail, _size);
                }
            }

            Unsafe.Free(_data, _allocator);
            _data = newPtr;
            _capacity = newCapacity;
            _head = 0;
            _tail = _count == newCapacity ? 0 : _count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MoveNext(ref int index)
        {
            var tmp = index + 1;
            if (tmp == _capacity)
            {
                tmp = 0;
            }

            index = tmp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Peek()
        {
            if (_count <= 0) throw new IndexOutOfRangeException();
            return ref Unsafe.GetArrayItem<T>(_data, _size, _head);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dequeue()
        {
            if (_count <= 0) throw new IndexOutOfRangeException();
            MoveNext(ref _head);
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

        public void Clear()
        {
            _count = 0;
            _head = 0;
            _tail = 0;
        }

        public void TrimExcess()
        {
            var newCapacity = (int) (_capacity * 0.9f);
            if (_count < newCapacity)
            {
                _capacity = _count;
                var newPtr = Unsafe.Malloc((ulong) _capacity * (ulong) _size, _allocator);
                Unsafe.CopyData(_data, 0, newPtr, 0, _count, _size);
                Unsafe.Free(_data, _allocator);
                _data = newPtr;
            }
        }

        public T[] ToArray()
        {
            if (_count == 0)
            {
                return Array.Empty<T>();
            }

            var arr = new T[_count];
            var arrSpan = new Span<T>(arr);
            if (_head < _tail)
            {
                var qSpan = MemoryMarshal.Cast<byte, T>(new Span<byte>(_data, _count * _size));
                qSpan.CopyTo(arrSpan);
            }
            else
            {
                var headSpan = MemoryMarshal.Cast<byte, T>(
                    new Span<byte>((byte*) _data + _head * _size, (_capacity - _head) * _size));
                var arrHead = arrSpan.Slice(0, _capacity - _head);
                headSpan.CopyTo(arrHead);
                var tailSpan = MemoryMarshal.Cast<byte, T>(
                    new Span<byte>((byte*) _data, _tail * _size));
                var arrTail = arrSpan.Slice(_capacity - _head, _tail);
                tailSpan.CopyTo(arrTail);
            }

            return arr;
        }

        public Enumerator GetEnumerator() { return new Enumerator(ref this); }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() { return GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly NativeQueue<T> _queue;
            private int _idx;

            public Enumerator(ref NativeQueue<T> queue)
            {
                _queue = queue;
                _idx = -1;
            }

            public ref T Current
            {
                get
                {
                    if (_idx == -2 || _idx == -1) throw new InvalidOperationException();
                    return ref Unsafe.GetArrayItem<T>(_queue._data, _queue._size, _idx);
                }
            }

            T IEnumerator<T>.Current => Current;
            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (_idx == -2) return false;
                _idx++;
                if (_idx == _queue._count)
                {
                    _idx = -2;
                    return false;
                }

                var index = _queue._head + _idx;
                if (index >= _queue._capacity)
                {
                    index -= _queue._capacity;
                }

                _idx = index;
                return true;
            }

            public void Reset() { _idx = -1; }

            public void Dispose() { _idx = -2; }
        }
    }
}