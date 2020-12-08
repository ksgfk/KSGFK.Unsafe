using System.Collections.Generic;
using System.Linq;

namespace KSGFK.Unsafe
{
    /// <summary>
    /// 参考https://github.com/timohausmann/quadtree-js
    /// 之前自己写的，速度很玄学，这个稳定些
    /// 不过这哥们的象限和我学的不太一样...他的是
    ///  3 | 2
    /// -------
    ///  4 | 1
    /// 我的是
    ///  2 | 1
    /// -------
    ///  3 | 4
    /// emm.......
    /// </summary>
    public class QuadTree<T>
    {
        private class Node
        {
            public List<(Aabb2D, T)> Data = new List<(Aabb2D, T)>(16);
            public int Four;
            public int Three;
            public int Two;
            public int One;
            public int Depth;
            public Aabb2D Bound;
            public bool HaveSubNode;
        }

        private readonly List<Node> _nodes;
        private readonly Queue<int> _usable;
        private readonly int _maxItem;
        private readonly int _maxDepth;

        public QuadTree(Aabb2D bound, int maxItem = 16, int maxDepth = 4)
        {
            _nodes = new List<Node>();
            _usable = new Queue<int>();
            _maxItem = maxItem;
            _maxDepth = maxDepth;
            _nodes.Add(new Node {Bound = bound, Depth = 1});
        }

        private int GetEmptyNode(Aabb2D rect, int depth)
        {
            int idx;
            if (_usable.Count == 0)
            {
                idx = _nodes.Count;
                _nodes.Add(new Node());
            }
            else
            {
                idx = _usable.Dequeue();
            }

            _nodes[idx].Bound = rect;
            _nodes[idx].Depth = depth;
            return idx;
        }

        private void Split(int idx)
        {
            var nextDepth = _nodes[idx].Depth + 1;
            var bound = _nodes[idx].Bound;
            var wm = bound.Width / 2;
            var hm = bound.Height / 2;
            var x = bound.Left;
            var y = bound.Down;
            var z = bound.Right;
            var w = bound.Up;
            _nodes[idx].Four = GetEmptyNode(new Aabb2D(x + wm, y, z, w - hm), nextDepth);
            _nodes[idx].Three = GetEmptyNode(new Aabb2D(x, y, z - wm, w - hm), nextDepth);
            _nodes[idx].Two = GetEmptyNode(new Aabb2D(x, y + hm, z - wm, w), nextDepth);
            _nodes[idx].One = GetEmptyNode(new Aabb2D(x + wm, y + hm, z, w), nextDepth);
            _nodes[idx].HaveSubNode = true;
        }

        private (bool, bool, bool, bool) GetInsertNode(Aabb2D pRect, int idx)
        {
            var bound = _nodes[idx].Bound;
            var wm = bound.Width / 2;
            var hm = bound.Height / 2;
            var verticalMidpoint = bound.Left + wm;
            var horizontalMidpoint = bound.Down + hm;
            var startIsNorth = pRect.Down < horizontalMidpoint;
            var startIsWest = pRect.Left < verticalMidpoint;
            var endIsEast = pRect.Right > verticalMidpoint;
            var endIsSouth = pRect.Up > horizontalMidpoint;
            return (endIsEast && endIsSouth,
                startIsWest && endIsSouth,
                startIsWest && startIsNorth,
                startIsNorth && endIsEast);
        }

        public void Add(Aabb2D rect, T item) { Add(rect, item, 0); }

        private void Add(Aabb2D rect, T item, int node)
        {
            var stack = new Stack<(Aabb2D, T, int)>(_maxDepth * 4);
            stack.Push((rect, item, node));
            while (stack.Count > 0)
            {
                var (rectT, itemT, nodeT) = stack.Pop();
                if (_nodes[nodeT].HaveSubNode)
                {
                    var (rt, lt, ld, rd) = GetInsertNode(rectT, nodeT);
                    if (rt) stack.Push((rectT, itemT, _nodes[nodeT].One));
                    if (lt) stack.Push((rectT, itemT, _nodes[nodeT].Two));
                    if (ld) stack.Push((rectT, itemT, _nodes[nodeT].Three));
                    if (rd) stack.Push((rectT, itemT, _nodes[nodeT].Four));
                    continue;
                }

                _nodes[nodeT].Data.Add((rectT, itemT));
                if (_nodes[nodeT].Data.Count > _maxItem && _nodes[nodeT].Depth <= _maxDepth)
                {
                    if (!_nodes[nodeT].HaveSubNode)
                    {
                        Split(nodeT);
                    }

                    foreach (var (r, i) in _nodes[nodeT].Data)
                    {
                        var (rt, lt, ld, rd) = GetInsertNode(r, nodeT);
                        if (rt) stack.Push((r, i, _nodes[nodeT].One));
                        if (lt) stack.Push((r, i, _nodes[nodeT].Two));
                        if (ld) stack.Push((r, i, _nodes[nodeT].Three));
                        if (rd) stack.Push((r, i, _nodes[nodeT].Four));
                    }

                    _nodes[nodeT].Data.Clear();
                }
            }
        }

        public T[] Retrieve(Aabb2D rect)
        {
            var set = new HashSet<T>();
            Retrieve(rect, 0, set);
            return set.ToArray();
        }

        public void Retrieve(Aabb2D rect, List<T> result)
        {
            var set = new HashSet<T>();
            Retrieve(rect, 0, set);
            result.Clear();
            result.AddRange(set);
        }

        private void Retrieve(Aabb2D rect, int node, HashSet<T> set)
        {
            var stack = new Stack<int>(_maxDepth * 4);
            stack.Push(node);
            while (stack.Count > 0)
            {
                var nodeT = stack.Pop();
                foreach (var (_, item) in _nodes[nodeT].Data)
                {
                    set.Add(item);
                }

                if (_nodes[nodeT].HaveSubNode)
                {
                    var (rt, lt, ld, rd) = GetInsertNode(rect, nodeT);
                    if (rt) stack.Push(_nodes[nodeT].One);
                    if (lt) stack.Push(_nodes[nodeT].Two);
                    if (ld) stack.Push(_nodes[nodeT].Three);
                    if (rd) stack.Push(_nodes[nodeT].Four);
                }
            }
        }

        public void Clear()
        {
            foreach (var node in _nodes)
            {
                node.Bound = default;
                node.Data.Clear();
                node.Depth = 0;
                node.Two = 0;
                node.Three = 0;
                node.Four = 0;
                node.One = 0;
                node.HaveSubNode = false;
            }

            for (var i = 1; i < _nodes.Count; i++)
            {
                _usable.Enqueue(i);
            }
        }

        public void TrimExcess()
        {
            var bound = _nodes[0].Bound;
            _nodes.Clear();
            _nodes.TrimExcess();
            _usable.Clear();
            _usable.TrimExcess();
            _nodes.Add(new Node {Bound = bound, Depth = 1});
        }
    }
}