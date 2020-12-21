using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace KSGFK.Unsafe
{
    [Obsolete("未测试")]
    public class AvlSet<T> : ISet<T>
    {
        public struct Enumerator : IEnumerator<T>
        {
            private AvlTree<T>.Enumerator _it;
            public T Current => _it.Current;
            object IEnumerator.Current => Current;

            public Enumerator(AvlSet<T> tree) { _it = tree._avl.GetEnumerator(); }

            public bool MoveNext() { return _it.MoveNext(); }

            public void Reset() { _it.Reset(); }

            public void Dispose() { _it.Dispose(); }
        }

        private readonly AvlTree<T> _avl;

        public int Count => _avl.Count;
        public bool IsReadOnly => false;
        public T Min => _avl.Min;
        public T Max => _avl.Max;
        public IComparer<T> Comparer => _avl.Comparer;

        public AvlSet() : this(null) { }

        public AvlSet(IComparer<T> comparer) { _avl = new AvlTree<T>(comparer); }

        public AvlSet(IEnumerable<T> e, IComparer<T> comparer = null) : this(comparer)
        {
            foreach (var item in e)
            {
                Add(item);
            }
        }

        public bool Add(T item) { return _avl.Add(item); }

        void ICollection<T>.Add(T item) { Add(item); }

        public void ExceptWith(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (Count == 0) return;
            if (ReferenceEquals(this, other))
            {
                Clear();
                return;
            }

            var min = Min;
            var max = Max;
            foreach (var item in other)
            {
                if (!(Comparer.Compare(item, min) < 0 || Comparer.Compare(item, max) > 0) && Contains(item))
                {
                    Remove(item);
                }
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (Count == 0) return;
            if (ReferenceEquals(this, other)) return;

            var toSave = new List<T>(Count);
            foreach (var item in other)
            {
                if (Contains(item))
                {
                    toSave.Add(item);
                }
            }

            Clear();
            foreach (var item in toSave)
            {
                Add(item);
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (other is ICollection c)
            {
                if (Count == 0) return c.Count > 0;
            }

            var hashSet = other.ToHashSet();
            if (hashSet.Count <= Count) return false;
            foreach (var item in this)
            {
                if (!hashSet.Contains(item)) return false;
            }

            return true;
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (Count == 0) return false;
            if (other is ICollection c && c.Count == 0) return true;

            var hashSet = other.ToHashSet();
            if (hashSet.Count >= Count) return false;
            foreach (var item in hashSet)
            {
                if (!Contains(item)) return false;
            }

            return true;
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (other is ICollection c)
            {
                if (Count == 0) return c.Count > 0;
            }

            var hashSet = other.ToHashSet();
            if (hashSet.Count < Count) return false;
            foreach (var item in this)
            {
                if (!hashSet.Contains(item)) return false;
            }

            return true;
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (Count == 0) return false;
            if (other is ICollection c && c.Count == 0) return true;

            var hashSet = other.ToHashSet();
            if (hashSet.Count > Count) return false;
            foreach (var item in hashSet)
            {
                if (!Contains(item)) return false;
            }

            return true;
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (Count == 0) return false;
            if (other is ICollection<T> c && c.Count == 0) return false;
            foreach (var item in other)
            {
                if (Contains(item)) return true;
            }

            return false;
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            var hashSet = other.ToHashSet();
            if (hashSet.Count != Count) return false;
            foreach (var item in hashSet)
            {
                if (!Contains(item)) return false;
            }

            return true;
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (Count == 0)
            {
                UnionWith(other);
                return;
            }

            if (ReferenceEquals(other, this))
            {
                Clear();
                return;
            }

            var elements = other.ToArray();
            Array.Sort(elements, 0, elements.Length, Comparer);
            var count = elements.Length;
            var previous = elements[0];
            for (var i = 0; i < count; i++)
            {
                while (i < count && i != 0 && Comparer.Compare(elements[i], previous) == 0) i++;
                if (i >= count) break;
                var current = elements[i];
                _ = Contains(current) ? Remove(current) : Add(current);
                previous = current;
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            foreach (var item in other)
            {
                Add(item);
            }
        }

        bool ISet<T>.Add(T item) { return Add(item); }

        public void Clear() { _avl.Clear(); }

        public bool Contains(T item) { return _avl.Contains(item); }

        public void CopyTo(T[] array, int arrayIndex)
        {
            var index = 0;
            foreach (var item in _avl)
            {
                array[arrayIndex + index] = item;
                index++;
            }
        }

        public bool Remove(T item) { return _avl.Remove(item); }

        public Enumerator GetEnumerator() { return new Enumerator(this); }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() { return GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }
}