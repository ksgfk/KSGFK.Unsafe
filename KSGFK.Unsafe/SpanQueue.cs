using System;
using System.Runtime.CompilerServices;

namespace KSGFK.Unsafe
{
    /// <summary>
    /// 封装在Span上的循环队列，容量不可变
    /// </summary>
    public ref struct SpanQueue<T>
    {
        private readonly Span<T> _span;
        private int _count;
        private int _head;
        private int _tail;

        /// <summary>
        /// 当前元素数量
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        /// <summary>
        /// 容量
        /// </summary>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _span.Length;
        }

        public SpanQueue(Span<T> span)
        {
            _span = span;
            _count = 0;
            _head = 0;
            _tail = 0;
        }

        public unsafe SpanQueue(void* ptr, int length) : this(new Span<T>(ptr, length)) { }

        /// <summary>
        /// 入队
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(T item)
        {
            ReSize();
            _span[_tail] = item;
            _count++;
            MoveNext(ref _tail);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReSize()
        {
            if (Count <= Capacity)
            {
                return;
            }
            throw new StackOverflowException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MoveNext(ref int index)
        {
            var tmp = index + 1;
            if (tmp == Capacity)
            {
                tmp = 0;
            }

            index = tmp;
        }

        /// <summary>
        /// 返回队首元素
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">没有元素</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Peek()
        {
            if (_count <= 0) throw new IndexOutOfRangeException();
            return ref _span[_head];
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">没有元素</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dequeue()
        {
            if (_count <= 0) throw new IndexOutOfRangeException();
            MoveNext(ref _head);
            _count--;
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        public void Clear()
        {
            _count = 0;
            _head = 0;
            _tail = 0;
        }

        /// <summary>
        /// 将所有队列元素转换成数组
        /// </summary>
        /// <returns></returns>
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
                Span<T> qSpan = _span[_head.._tail];
                qSpan.CopyTo(arrSpan);
            }
            else
            {
                var headSpan = _span[_head..Capacity];
                var arrHead = arrSpan[..(Capacity - _head)];
                headSpan.CopyTo(arrHead);
                var tailSpan = _span[.._tail];
                var arrTail = arrSpan.Slice(Capacity - _head, _tail);
                tailSpan.CopyTo(arrTail);
            }
            return arr;
        }

        public Enumerator GetEnumerator() { return new Enumerator(ref this); }

        public ref struct Enumerator
        {
            private readonly SpanQueue<T> _queue;
            private int _idx;

            public Enumerator(ref SpanQueue<T> queue)
            {
                _queue = queue;
                _idx = -1;
            }

            public ref T Current
            {
                get
                {
                    if (_idx == -2 || _idx == -1) throw new InvalidOperationException();
                    return ref _queue._span[_idx];
                }
            }

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
                if (index >= _queue.Capacity)
                {
                    index -= _queue.Capacity;
                }
                _idx = index;
                return true;
            }

            public void Reset() { _idx = -1; }

            public void Dispose() { _idx = -2; }
        }
    }
}
