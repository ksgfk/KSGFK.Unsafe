using System;
using System.Collections.Generic;

namespace KSGFK.Unsafe
{
    public class QuadTree<T>
    {
        private sealed class Node
        {
            private List<AABB2D> _aabbs;
            private List<T> _data;
            public Node RightTop;
            public Node LeftTop;
            public Node LeftDown;
            public Node RightDown;
            public readonly AABB2D Size;
            public List<T> Data => _data;
            public List<AABB2D> AABB => _aabbs;

            public Node(AABB2D size)
            {
                Size = size;
                _aabbs = null;
            }

            public void Add(AABB2D aabb, T data)
            {
                (_aabbs ??= new List<AABB2D>()).Add(aabb);
                (_data ??= new List<T>()).Add(data);
            }

            public void Clean()
            {
                _aabbs.Clear();
                _data.Clear();
            }
        }

        private Node _root;
        private readonly AABB2D _worldSize;
        private readonly int _maxDepth;

        public QuadTree(AABB2D worldSize, int maxDepth)
        {
            _worldSize = worldSize;
            _maxDepth = maxDepth;
            _root = null;
        }

        public void Add(AABB2D aabb, T data)
        {
            _root ??= new Node(_worldSize);
            Add(aabb, data, _root, 1);
        }

        public void CollisionTest(AABB2D aabb, List<T> data)
        {
            if (_root == null) return;
            CollisionTest(aabb, data, _root, 1);
        }

        private void Add(AABB2D aabb, T data, Node nodeT, int depthT)
        {
            var stack = new Stack<(Node, int)>(Math.Max((int) Math.Pow(2, _maxDepth), 100));
            stack.Push((nodeT, depthT));
            while (stack.Count > 0)
            {
                var (node, depth) = stack.Pop();
                var size = node.Size;
                if (size.IsCross(aabb))
                {
                    if (!size.Contains(aabb) || depth >= _maxDepth)
                    {
                        node.Add(aabb, data);
                    }

                    if (depth >= _maxDepth)
                    {
                        // return;
                        continue;
                    }

                    var w = size.Width / 2f;
                    var h = size.Height / 2f;
                    node.RightTop ??= new Node(new AABB2D(size.Left + w, size.Top, size.Right, size.Down + h));
                    stack.Push((node.RightTop, depth + 1));
                    // Add(aabb, data, node.RightTop, depth + 1);
                    node.LeftTop ??= new Node(new AABB2D(size.Left, size.Top, size.Right - w, size.Down + h));
                    stack.Push((node.LeftTop, depth + 1));
                    // Add(aabb, data, node.LeftTop, depth + 1);
                    node.LeftDown ??= new Node(new AABB2D(size.Left, size.Top - h, size.Right - w, size.Down));
                    stack.Push((node.LeftDown, depth + 1));
                    // Add(aabb, data, node.LeftDown, depth + 1);
                    node.RightDown ??= new Node(new AABB2D(size.Left + w, size.Top - h, size.Right, size.Down));
                    stack.Push((node.RightDown, depth + 1));
                    // Add(aabb, data, node.RightDown, depth + 1);
                }
            }
        }

        private void CollisionTest(AABB2D aabb, List<T> data, Node nodeT, int depthT)
        {
            var stack = new Stack<(Node, int)>(Math.Max((int) Math.Pow(2, _maxDepth), 100));
            var set = new HashSet<T>();
            stack.Push((nodeT, depthT));
            while (stack.Count > 0)
            {
                var (node, depth) = stack.Pop();
                var size = node.Size;
                if (size.IsCross(aabb))
                {
                    if ((!size.Contains(aabb) || depth >= _maxDepth) && node.Data != null)
                    {
                        for (var i = 0; i < node.AABB.Count; i++)
                        {
                            if (node.AABB[i].IsCross(aabb))
                            {
                                set.Add(node.Data[i]);
                            }
                        }
                    }

                    if (depth >= _maxDepth)
                    {
                        // return;
                        continue;
                    }

                    // if (node.RightTop != null) CollisionTest(aabb, data, node.RightTop, depth + 1);
                    if (node.RightTop != null) stack.Push((node.RightTop, depth + 1));
                    // if (node.LeftTop != null) CollisionTest(aabb, data, node.LeftTop, depth + 1);
                    if (node.LeftTop != null) stack.Push((node.LeftTop, depth + 1));
                    // if (node.LeftDown != null) CollisionTest(aabb, data, node.LeftDown, depth + 1);
                    if (node.LeftDown != null) stack.Push((node.LeftDown, depth + 1));
                    // if (node.RightDown != null) CollisionTest(aabb, data, node.RightDown, depth + 1);
                    if (node.RightDown != null) stack.Push((node.RightDown, depth + 1));
                }
            }

            data.Clear();
            data.AddRange(set);
        }

        public void Clear() { Clear(_root); }

        private void Clear(Node node)
        {
            if (node == null) return;
            node.Clean();
            Clear(node.LeftDown);
            Clear(node.LeftTop);
            Clear(node.RightDown);
            Clear(node.RightTop);
        }
    }
}