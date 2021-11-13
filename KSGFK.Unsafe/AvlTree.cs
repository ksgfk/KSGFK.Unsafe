using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace KSGFK.Unsafe
{
    public class AvlTree<T> : IEnumerable<T>
    {
        internal class Node
        {
            public Node Left;
            public Node Right;
            public Node Parent;
            public int Height;
            public T Item { get; internal set; }
            public Node(T item) { Item = item; }
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly AvlTree<T> _tree;
            private readonly Stack<Node> _stack;
            private Node _current;

            public T Current => _current.Item;

            object IEnumerator.Current => Current;

            internal Enumerator(AvlTree<T> tree)
            {
                _tree = tree;
                _stack = _tree._root != null ? new Stack<Node>(_tree._root.Height) : new Stack<Node>();
                _current = null;
                Init();
            }

            private void Init()
            {
                var temp = _tree._root;
                while (temp != null)
                {
                    var next = temp.Left;
                    _stack.Push(temp);
                    temp = next;
                }
            }

            public bool MoveNext()
            {
                if (_stack.Count == 0)
                {
                    _current = null;
                    return false;
                }

                _current = _stack.Pop();
                var node = _current.Right;
                while (node != null)
                {
                    var next = node.Left;
                    _stack.Push(node);
                    node = next;
                }

                return true;
            }

            public void Reset()
            {
                _stack.Clear();
                _current = null;
                Init();
            }

            public void Dispose() { }
        }

        private readonly IComparer<T> _comparer;
        private Node _root;
        private int _count;

        public int Count => _count;
        public IComparer<T> Comparer => _comparer;

        public T Min
        {
            get
            {
                if (_root == null) return default;
                var temp = _root;
                while (temp.Left != null)
                {
                    temp = temp.Left;
                }

                return temp.Item;
            }
        }

        public T Max
        {
            get
            {
                if (_root == null) return default;
                var temp = _root;
                while (temp.Right != null)
                {
                    temp = temp.Right;
                }

                return temp.Item;
            }
        }

        public int MaxHeight => GetMaxHeight(_root);
        public int RecordHeight => _root?.Height ?? 0;

        public AvlTree() : this(null) { }

        public AvlTree(IComparer<T> comparer)
        {
            _comparer = comparer ?? Comparer<T>.Default;
            _root = null;
            _count = 0;
        }

        public AvlTree(IEnumerable<T> e, IComparer<T> comparer = null) : this(comparer)
        {
            foreach (var item in e)
            {
                Add(item);
            }
        }

        private static int GetMaxHeight(Node node)
        {
            if (node == null) return 0;
            return 1 + Math.Max(GetMaxHeight(node.Left), GetMaxHeight(node.Right));
        }

        public bool Add(T item)
        {
            var node = new Node(item);
            var temp = _root;
            Node parent = null;
            var lOrR = false;
            while (temp != null)
            {
                parent = temp;
                var cmp = _comparer.Compare(item, temp.Item);
                if (cmp == 0) return false;
                if (cmp < 0)
                {
                    temp = temp.Left;
                    lOrR = false;
                }
                else
                {
                    temp = temp.Right;
                    lOrR = true;
                }
            }

            if (parent == null)
            {
                _root = node;
            }
            else
            {
                if (!lOrR) parent.Left = node;
                else parent.Right = node;
            }

            node.Parent = parent;
            Insert(node);
            _count++;
            return true;
        }

        public bool Contains(T item) { return FindNodeInternal(item) != null; }

        public void Remove(AvlTreeNode<T> node)
        {
            if (node.Tree != this) throw new ArgumentException();
            if (!node.HasValue) return;
            Erase(node.Node);
        }

        public bool Remove(T item)
        {
            var node = FindNodeInternal(item);
            if (node == null) return false;
            Erase(node);
            return true;
        }

        public bool TryGetValue(T value, out T equalValue)
        {
            var node = FindNodeInternal(value);
            if (node == null)
            {
                equalValue = default;
                return false;
            }

            equalValue = node.Item;
            return true;
        }

        public AvlTreeNode<T> FindNode(T item) { return new AvlTreeNode<T>(this, FindNodeInternal(item)); }

        public AvlTreeNode<T> NextNode(AvlTreeNode<T> treeNode)
        {
            if (treeNode.Tree != this) throw new ArgumentException();
            var node = treeNode.Node;
            if (!treeNode.HasValue) return new AvlTreeNode<T>(this, null);
            if (node.Right != null)
            {
                node = node.Right;
                while (node.Left != null) node = node.Left;
            }
            else
            {
                while (true)
                {
                    var last = node;
                    node = node.Parent;
                    if (node == null) break;
                    if (node.Left == last) break;
                }
            }

            return new AvlTreeNode<T>(this, node);
        }

        public AvlTreeNode<T> PreviousNode(AvlTreeNode<T> treeNode)
        {
            if (treeNode.Tree != this) throw new ArgumentException();
            var node = treeNode.Node;
            if (!treeNode.HasValue) return new AvlTreeNode<T>(this, null);
            if (node.Left != null)
            {
                node = node.Left;
                while (node.Right != null) node = node.Right;
            }
            else
            {
                while (true)
                {
                    var last = node;
                    node = node.Parent;
                    if (node == null) break;
                    if (node.Right == last) break;
                }
            }

            return new AvlTreeNode<T>(this, node);
        }

        public T NearestPrevious(T item, T min)
        {
            var now = _root;
            var res = min;
            while (now != null)
            {
                if (_comparer.Compare(now.Item, item) < 0 && _comparer.Compare(now.Item, res) > 0)
                {
                    res = now.Item;
                }

                now = _comparer.Compare(now.Item, item) >= 0 ? now.Left : now.Right;
            }

            return res;
        }

        public T NearestNext(T item, T max)
        {
            var now = _root;
            var res = max;
            while (now != null)
            {
                if (_comparer.Compare(now.Item, item) > 0 && _comparer.Compare(now.Item, res) < 0)
                {
                    res = now.Item;
                }

                now = _comparer.Compare(now.Item, item) > 0 ? now.Left : now.Right;
            }

            return res;
        }

        public void Clear()
        {
            _root = null;
            _count = 0;
        }

        public Enumerator GetEnumerator() { return new Enumerator(this); }

        public void CopyTo(T[] array, int arrayIndex)
        {
            var index = 0;
            foreach (var item in this)
            {
                array[arrayIndex + index] = item;
                index++;
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() { return GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        private Node FindNodeInternal(T item)
        {
            var temp = _root;
            while (temp != null)
            {
                var cmp = _comparer.Compare(item, temp.Item);
                if (cmp == 0) return temp;
                temp = cmp < 0 ? temp.Left : temp.Right;
            }

            return null;
        }

        private static int GetLeftNodeHeight(Node node) { return node.Left?.Height ?? 0; }

        private static int GetRightNodeHeight(Node node) { return node.Right?.Height ?? 0; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Update(Node node)
        {
            var leftHeight = GetLeftNodeHeight(node);
            var rightHeight = GetRightNodeHeight(node);
            node.Height = Math.Max(leftHeight, rightHeight) + 1;
        }

        private void ReplaceChild(Node oldChild, Node newChild, Node parent)
        {
            if (parent != null)
            {
                if (parent.Left == oldChild) parent.Left = newChild;
                else parent.Right = newChild;
            }
            else
            {
                _root = newChild;
            }
        }

        private Node RotateRight(Node node)
        {
            var left = node.Left;
            var parent = node.Parent;
            node.Left = left.Right;
            if (left.Right != null) left.Right.Parent = node;
            left.Right = node;
            left.Parent = parent;
            ReplaceChild(node, left, parent);
            node.Parent = left;
            return left;
        }

        private Node RotateLeft(Node node)
        {
            var right = node.Right;
            var parent = node.Parent;
            node.Right = right.Left;
            if (right.Left != null) right.Left.Parent = node;
            right.Left = node;
            right.Parent = parent;
            ReplaceChild(node, right, parent);
            node.Parent = right;
            return right;
        }

        private Node BalanceLeft(Node node)
        {
            var right = node.Right;
            var leftHeight = GetLeftNodeHeight(right);
            var rightHeight = GetRightNodeHeight(right);
            if (leftHeight > rightHeight)
            {
                right = RotateRight(right);
                Update(right.Right);
                Update(right);
            }

            node = RotateLeft(node);
            Update(node.Left);
            Update(node);
            return node;
        }

        private Node BalanceRight(Node node)
        {
            var left = node.Left;
            var leftHeight = GetLeftNodeHeight(left);
            var rightHeight = GetRightNodeHeight(left);
            if (leftHeight < rightHeight)
            {
                left = RotateLeft(left);
                Update(left.Left);
                Update(left);
            }

            node = RotateRight(node);
            Update(node.Right);
            Update(node);
            return node;
        }

        private void Insert(Node node)
        {
            node.Height = 1;
            for (node = node.Parent; node != null; node = node.Parent)
            {
                var leftHeight = GetLeftNodeHeight(node);
                var rightHeight = GetRightNodeHeight(node);
                var height = Math.Max(leftHeight, rightHeight) + 1;
                var balance = leftHeight - rightHeight;
                if (node.Height == height) break;
                node.Height = height;
                if (balance <= -2)
                {
                    node = BalanceLeft(node);
                }
                else if (balance >= 2)
                {
                    node = BalanceRight(node);
                }
            }
        }

        private void ReBalance(Node node)
        {
            while (node != null)
            {
                var leftHeight = GetLeftNodeHeight(node);
                var rightHeight = GetRightNodeHeight(node);
                var balance = leftHeight - rightHeight;
                var height = Math.Max(leftHeight, rightHeight) + 1;
                if (node.Height != height)
                {
                    node.Height = height;
                }
                else if (balance >= -1 && balance <= 1)
                {
                    break;
                }

                if (balance <= -2)
                {
                    node = BalanceLeft(node);
                }
                else if (balance >= 2)
                {
                    node = BalanceRight(node);
                }

                node = node.Parent;
            }
        }

        private void Erase(Node node)
        {
            Node child;
            Node parent;
            if (node.Left != null && node.Right != null)
            {
                var old = node;
                node = node.Right;
                var left = node.Left;
                while (left != null)
                {
                    node = left;
                    left = node.Left;
                }

                child = node.Right;
                parent = node.Parent;
                if (child != null)
                {
                    child.Parent = parent;
                }

                ReplaceChild(node, child, parent);
                if (node.Parent == old) parent = node;
                node.Left = old.Left;
                node.Right = old.Right;
                node.Parent = old.Parent;
                node.Height = old.Height;
                ReplaceChild(old, node, old.Parent);
                old.Left.Parent = node;
                if (old.Right != null)
                {
                    old.Right.Parent = node;
                }
            }
            else
            {
                child = node.Left ?? node.Right;
                parent = node.Parent;
                ReplaceChild(node, child, parent);
                if (child != null)
                {
                    child.Parent = parent;
                }
            }

            if (parent != null)
            {
                ReBalance(parent);
            }

            _count--;
        }

        public void CheckHeight()
        {
            var stack = new Stack<Node>();

            var temp = _root;
            while (temp != null)
            {
                var next = temp.Left;
                stack.Push(temp);
                temp = next;
            }

            while (stack.Count > 0)
            {
                var n = stack.Pop();
                var l = n.Left;
                var rn = n.Right;
                var lh = l?.Height ?? 0;
                var rh = rn?.Height ?? 0;
                var h = Math.Max(lh, rh) + 1;
                if (n.Height != h)
                {
                    throw new ArgumentException();
                }

                if (Math.Abs(lh - rh) >= 2)
                {
                    throw new ArgumentException();
                }

                var r = n.Right;
                while (r != null)
                {
                    var next = r.Left;
                    stack.Push(r);
                    r = next;
                }
            }
        }
    }

    public readonly struct AvlTreeNode<T>
    {
        internal readonly AvlTree<T> Tree;
        internal readonly AvlTree<T>.Node Node;

        public T Value => Node.Item;
        public bool HasValue => Node != null;

        internal AvlTreeNode(AvlTree<T> tree, AvlTree<T>.Node node)
        {
            Tree = tree;
            Node = node;
        }

        public override string ToString() { return HasValue ? Value.ToString() : "null"; }
    }

    [Obsolete("only for test")]
    public class AvlDictionary<TK, TV>
    {
        private class PairComparer : Comparer<KeyValuePair<TK, TV>>
        {
            private readonly IComparer<TK> _keyComparer;

            public PairComparer(IComparer<TK> keyComparer)
            {
                _keyComparer = (keyComparer ?? Comparer<TK>.Default);
            }

            public override int Compare(KeyValuePair<TK, TV> x, KeyValuePair<TK, TV> y)
            {
                return _keyComparer.Compare(x.Key, y.Key);
            }
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TK, TV>>, IEnumerator, IDisposable
        {
            private AvlTree<KeyValuePair<TK, TV>>.Enumerator _enumerator;

            public KeyValuePair<TK, TV> Current => _enumerator.Current;

            object IEnumerator.Current => Current;

            internal Enumerator(AvlDictionary<TK, TV> avl)
            {
                _enumerator = avl._tree.GetEnumerator();
            }

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            public void Reset()
            {
                _enumerator.Reset();
            }

            public void Dispose()
            {
                _enumerator.Dispose();
            }
        }

        public readonly AvlTree<KeyValuePair<TK, TV>> _tree;

        public TV this[TK key]
        {
            get
            {
                AvlTreeNode<KeyValuePair<TK, TV>> result = _tree.FindNode(new KeyValuePair<TK, TV>(key, default(TV)));
                if (!result.HasValue)
                {
                    throw new ArgumentException();
                }
                return result.Value.Value;
            }
            set
            {
                AvlTreeNode<KeyValuePair<TK, TV>> result = _tree.FindNode(new KeyValuePair<TK, TV>(key, default(TV)));
                if (!result.HasValue)
                {
                    _tree.Add(new KeyValuePair<TK, TV>(key, value));
                }
                else
                {
                    result.Node.Item = new KeyValuePair<TK, TV>(key, value);
                }
            }
        }

        public AvlDictionary() : this(null)
        {
        }

        public AvlDictionary(IComparer<TK> comparer)
        {
            _tree = new AvlTree<KeyValuePair<TK, TV>>(new PairComparer(comparer));
        }

        public void Add(TK key, TV value)
        {
            if (!_tree.Add(new KeyValuePair<TK, TV>(key, value)))
            {
                throw new ArgumentException();
            }
        }

        public bool Remove(TK key)
        {
            return _tree.Remove(new KeyValuePair<TK, TV>(key, default(TV)));
        }

        public void Remove(AvlTreeNode<KeyValuePair<TK, TV>> node)
        {
            _tree.Remove(node);
        }

        public void Clear()
        {
            _tree.Clear();
        }

        public bool ContainsKey(TK key)
        {
            return _tree.Contains(new KeyValuePair<TK, TV>(key, default(TV)));
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public AvlTreeNode<KeyValuePair<TK, TV>> FindNode(TK key)
        {
            return _tree.FindNode(new KeyValuePair<TK, TV>(key, default(TV)));
        }

        public void SetValue(AvlTreeNode<KeyValuePair<TK, TV>> node, TV newValue)
        {
            node.Node.Item = new KeyValuePair<TK, TV>(node.Value.Key, newValue);
        }

        public AvlTreeNode<KeyValuePair<TK, TV>> PreviousNode(AvlTreeNode<KeyValuePair<TK, TV>> treeNode)
        {
            return _tree.PreviousNode(treeNode);
        }

        public AvlTreeNode<KeyValuePair<TK, TV>> NextNode(AvlTreeNode<KeyValuePair<TK, TV>> treeNode)
        {
            return _tree.NextNode(treeNode);
        }

        public KeyValuePair<TK, TV> NearestUpper(TK key, TK min)
        {
            return _tree.NearestPrevious(new KeyValuePair<TK, TV>(key, default(TV)), new KeyValuePair<TK, TV>(min, default(TV)));
        }

        public KeyValuePair<TK, TV> NearestLower(TK key, TK max)
        {
            return _tree.NearestNext(new KeyValuePair<TK, TV>(key, default(TV)), new KeyValuePair<TK, TV>(max, default(TV)));
        }
    }
}