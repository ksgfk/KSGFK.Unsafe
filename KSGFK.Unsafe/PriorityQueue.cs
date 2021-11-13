using System;
using System.Collections.Generic;

namespace KSGFK.Unsafe
{
    public class PriorityQueue<T>
    {
        private readonly List<T> _data;
        private readonly IComparer<T> _cmp;

        public int Count => _data.Count;
        public int Capacity => _data.Capacity;
        public bool IsEmpty => _data.Count <= 0;

        public PriorityQueue() : this(8, Comparer<T>.Default) { }

        public PriorityQueue(int size) : this(size, Comparer<T>.Default) { }

        public PriorityQueue(int size, IComparer<T> cmp)
        {
            _data = new List<T>(size);
            _cmp = cmp;
        }

        public void Enqueue(T item)
        {
            _data.Add(item);
            PushHeap(_data, _cmp);
        }

        public void Dequeue()
        {
            PopHeap(_data, _cmp);
            _data.RemoveAt(_data.Count - 1);
        }

        public T Peek()
        {
            if (_data.Count <= 0) throw new IndexOutOfRangeException();
            return _data[0];
        }

        public void Clear() { _data.Clear(); }

        public void TrimExcess() { _data.TrimExcess(); }

        public static void PushHeap(IList<T> list, IComparer<T> pred)
        {
            var count = list.Count;
            if (count < 0 || count >= int.MaxValue) throw new IndexOutOfRangeException();
            if (count >= 2)
            {
                var uLast = count - 1;
                var val = list[uLast];
                PushHeapByIndex(list, uLast, 0, val, pred);
            }
        }

        private static void PushHeapByIndex(IList<T> list, int hole, int top, T value, IComparer<T> pred)
        {
            for (var idx = (hole - 1) >> 1;
                top < hole && pred.Compare(value, list[idx]) < 0;
                idx = (hole - 1) >> 1)
            {
                list[hole] = list[idx];
                hole = idx;
            }

            list[hole] = value;
        }

        public static void PopHeap(IList<T> list, IComparer<T> pred)
        {
            var count = list.Count;
            if (count < 0 || count >= int.MaxValue) throw new IndexOutOfRangeException();
            if (count >= 2)
            {
                var uLast = count - 1;
                var val = list[uLast];
                list[uLast] = list[0];
                var hole = 0;
                var bottom = uLast;
                var top = hole;
                var idx = hole;
                var maxSequenceNonLeaf = (bottom - 1) >> 1;
                while (idx < maxSequenceNonLeaf)
                {
                    idx = 2 * idx + 2;
                    if (pred.Compare(list[idx - 1], list[idx]) < 0)
                    {
                        --idx;
                    }

                    list[hole] = list[idx];
                    hole = idx;
                }

                if (idx == maxSequenceNonLeaf && bottom % 2 == 0)
                {
                    list[hole] = list[bottom - 1];
                    hole = bottom - 1;
                }

                PushHeapByIndex(list, hole, top, val, pred);
            }
        }
    }
}