using System;
using System.Runtime.CompilerServices;

namespace KSGFK.Unsafe
{
    /// <summary>
    /// 封装在Span上的栈，容量不可变
    /// </summary>
    public ref struct SpanStack<T>
    {
        private readonly Span<T> _span;
        private int _count;

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

        public SpanStack(Span<T> span)
        {
            _span = span;
            _count = 0;
        }

        public unsafe SpanStack(void* ptr, int length) : this(new Span<T>(ptr, length)) { }

        /// <summary>
        /// 入栈
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(T item)
        {
            var last = _count;
            _count++;
            if (Count > Capacity)
            {
                throw new StackOverflowException();
            }
            _span[last] = item;
        }

        /// <summary>
        /// 返回队首元素
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">没有元素</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Peek()
        {
            if (_count <= 0) throw new IndexOutOfRangeException();
            return ref _span[_count - 1];
        }

        /// <summary>
        /// 出栈
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">没有元素</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pop()
        {
            if (_count <= 0) throw new IndexOutOfRangeException();
            _count--;
        }

        /// <summary>
        /// 清空栈
        /// </summary>
        public void Clear() { _count = 0; }

        public T[] ToArray()
        {
            if (_count == 0)
            {
                return Array.Empty<T>();
            }
            var hasValueSpan = _span[0.._count];
            var arr = hasValueSpan.ToArray();
            arr.AsSpan().Reverse();//标准库里是从栈顶往下ToArray...
            return arr;
        }

        public Enumerator GetEnumerator() { return new Enumerator(ref this); }

        public ref struct Enumerator
        {
            private readonly SpanStack<T> _stack;
            private int _idx;

            public ref T Current
            {
                get
                {
                    if (_idx == -2 || _idx == -1) throw new InvalidOperationException();
                    return ref _stack._span[_idx];
                }
            }

            public Enumerator(ref SpanStack<T> stack)
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
