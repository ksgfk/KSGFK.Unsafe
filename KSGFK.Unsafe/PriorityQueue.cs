using System;
using System.Collections.Generic;

namespace KSGFK.Collections
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
            Unsafe.Unsafe.PushHeap(_data, _cmp);
        }

        public void Dequeue()
        {
            Unsafe.Unsafe.PopHeap(_data, _cmp);
            _data.RemoveAt(_data.Count - 1);
        }

        public T Peek()
        {
            if (_data.Count <= 0) throw new IndexOutOfRangeException();
            return _data[0];
        }

        public void Clear() { _data.Clear(); }

        public void TrimExcess() { _data.TrimExcess(); }
    }
}