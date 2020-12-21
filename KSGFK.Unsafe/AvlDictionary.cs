using System;
using System.Collections;
using System.Collections.Generic;

namespace KSGFK.Unsafe
{
    [Obsolete("未测试")]
    public class AvlDictionary<TK, TV> : IDictionary<TK, TV>
    {
        public class KeyValuePairComparer : IComparer<KeyValuePair<TK, TV>>
        {
            private readonly IComparer<TK> _comparer;

            public KeyValuePairComparer(IComparer<TK> comparer) { _comparer = comparer; }

            public int Compare(KeyValuePair<TK, TV> x, KeyValuePair<TK, TV> y)
            {
                return _comparer.Compare(x.Key, y.Key);
            }
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TK, TV>>
        {
            private AvlTree<KeyValuePair<TK, TV>>.Enumerator _enumerator;

            public KeyValuePair<TK, TV> Current => _enumerator.Current;
            object IEnumerator.Current => Current;

            internal Enumerator(AvlTree<KeyValuePair<TK, TV>> set) { _enumerator = set.GetEnumerator(); }

            public bool MoveNext() { return _enumerator.MoveNext(); }

            public void Reset() { _enumerator.Reset(); }

            public void Dispose() { _enumerator.Dispose(); }
        }

        public readonly struct KeyCollection : ICollection<TK>
        {
            public struct KeyEnumerator : IEnumerator<TK>
            {
                private Enumerator _it;
                public TK Current => _it.Current.Key;
                object IEnumerator.Current => Current;

                public KeyEnumerator(AvlDictionary<TK, TV> tree) { _it = tree.GetEnumerator(); }

                public bool MoveNext() { return _it.MoveNext(); }

                public void Reset() { _it.Reset(); }

                public void Dispose() { _it.Dispose(); }
            }

            private readonly AvlDictionary<TK, TV> _dict;

            public int Count => _dict.Count;
            public bool IsReadOnly => true;

            public KeyCollection(AvlDictionary<TK, TV> dict) { _dict = dict; }

            public KeyEnumerator GetEnumerator() { return new KeyEnumerator(_dict); }

            IEnumerator<TK> IEnumerable<TK>.GetEnumerator() { return GetEnumerator(); }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            void ICollection<TK>.Add(TK item) { throw new NotSupportedException(); }

            void ICollection<TK>.Clear() { throw new NotSupportedException(); }

            public bool Contains(TK item) { return _dict.ContainsKey(item); }

            public void CopyTo(TK[] array, int arrayIndex)
            {
                var index = 0;
                foreach (var (k, _) in _dict)
                {
                    array[arrayIndex + index] = k;
                    index++;
                }
            }

            bool ICollection<TK>.Remove(TK item) { throw new NotSupportedException(); }
        }

        public readonly struct ValueCollection : ICollection<TV>
        {
            public struct ValueEnumerator : IEnumerator<TV>
            {
                private Enumerator _it;
                public TV Current => _it.Current.Value;
                object IEnumerator.Current => Current;

                public ValueEnumerator(AvlDictionary<TK, TV> tree) { _it = tree.GetEnumerator(); }

                public bool MoveNext() { return _it.MoveNext(); }

                public void Reset() { _it.Reset(); }

                public void Dispose() { _it.Dispose(); }
            }

            private readonly AvlDictionary<TK, TV> _dict;

            public int Count => _dict.Count;
            public bool IsReadOnly => true;

            public ValueCollection(AvlDictionary<TK, TV> dict) { _dict = dict; }

            public ValueEnumerator GetEnumerator() { return new ValueEnumerator(_dict); }

            IEnumerator<TV> IEnumerable<TV>.GetEnumerator() { return GetEnumerator(); }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            void ICollection<TV>.Add(TV item) { throw new NotSupportedException(); }

            void ICollection<TV>.Clear() { throw new NotSupportedException(); }

            public bool Contains(TV item) { return _dict.ContainsValue(item); }

            public void CopyTo(TV[] array, int arrayIndex)
            {
                var index = 0;
                foreach (var (_, v) in _dict)
                {
                    array[arrayIndex + index] = v;
                    index++;
                }
            }

            bool ICollection<TV>.Remove(TV item) { throw new NotSupportedException(); }
        }

        private readonly AvlTree<KeyValuePair<TK, TV>> _avl;

        public int Count => _avl.Count;
        public bool IsReadOnly => false;
        public IComparer<TK> Comparer { get; }
        public TK MaxKey => _avl.Max.Key;
        public TV MaxValue => _avl.Max.Value;
        public TK MinKey => _avl.Min.Key;
        public TV MinValue => _avl.Min.Value;
        public int MaxHeight => _avl.MaxHeight;

        public KeyCollection Keys { get; }
        ICollection<TK> IDictionary<TK, TV>.Keys => Keys;
        public ValueCollection Values { get; }
        ICollection<TV> IDictionary<TK, TV>.Values => Values;

        public TV this[TK key]
        {
            get
            {
                var node = _avl.FindNode(new KeyValuePair<TK, TV>(key, default));
                if (!node.HasValue) throw new ArgumentException();
                return node.Value.Value;
            }
            set
            {
                var node = _avl.FindNode(new KeyValuePair<TK, TV>(key, default));
                if (!node.HasValue) throw new ArgumentException();
                node.Node.Item = new KeyValuePair<TK, TV>(key, value);
            }
        }

        public AvlDictionary() : this(null) { }

        public AvlDictionary(IComparer<TK> comparer)
        {
            comparer ??= Comparer<TK>.Default;
            Comparer = comparer;
            _avl = new AvlTree<KeyValuePair<TK, TV>>(new KeyValuePairComparer(comparer));
            Keys = new KeyCollection(this);
            Values = new ValueCollection(this);
        }

        public Enumerator GetEnumerator() { return new Enumerator(_avl); }

        IEnumerator<KeyValuePair<TK, TV>> IEnumerable<KeyValuePair<TK, TV>>.GetEnumerator() { return GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        void ICollection<KeyValuePair<TK, TV>>.Add(KeyValuePair<TK, TV> item) { Add(item.Key, item.Value); }

        public void Clear() { _avl.Clear(); }

        bool ICollection<KeyValuePair<TK, TV>>.Contains(KeyValuePair<TK, TV> item) { return ContainsKey(item.Key); }

        public void CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex) { _avl.CopyTo(array, arrayIndex); }

        bool ICollection<KeyValuePair<TK, TV>>.Remove(KeyValuePair<TK, TV> item) { return Remove(item.Key); }

        public void Add(TK key, TV value) { _avl.Add(new KeyValuePair<TK, TV>(key, value)); }

        public bool ContainsKey(TK key) { return _avl.Contains(new KeyValuePair<TK, TV>(key, default)); }

        public bool ContainsValue(TV value)
        {
            var cmp = Comparer<TV>.Default;
            foreach (var (_, v) in _avl)
            {
                if (cmp.Compare(v, value) == 0) return true;
            }

            return false;
        }

        public bool Remove(TK key) { return _avl.Remove(new KeyValuePair<TK, TV>(key, default)); }

        public bool TryGetValue(TK key, out TV value)
        {
            var result = _avl.TryGetValue(new KeyValuePair<TK, TV>(key, default), out var pair);
            value = pair.Value;
            return result;
        }
    }
}