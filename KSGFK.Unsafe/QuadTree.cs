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
            public List<(BoundingBox2F, T)> Data = new List<(BoundingBox2F, T)>(16);
            public int Four;
            public int Three;
            public int Two;
            public int One;
            public int Depth;
            public BoundingBox2F Bound;
            public bool HaveSubNode;
        }

        private readonly List<Node> _nodes;
        private readonly Queue<int> _usable;
        private readonly int _maxItem;
        private readonly int _maxDepth;

        public QuadTree(BoundingBox2F bound, int maxItem = 16, int maxDepth = 4)
        {
            _nodes = new List<Node>();
            _usable = new Queue<int>();
            _maxItem = maxItem;
            _maxDepth = maxDepth;
            _nodes.Add(new Node {Bound = bound, Depth = 1});
        }

        private int GetEmptyNode(BoundingBox2F rect, int depth)
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
            _nodes[idx].Four = GetEmptyNode(new BoundingBox2F(x + wm, y, z, w - hm), nextDepth);
            _nodes[idx].Three = GetEmptyNode(new BoundingBox2F(x, y, z - wm, w - hm), nextDepth);
            _nodes[idx].Two = GetEmptyNode(new BoundingBox2F(x, y + hm, z - wm, w), nextDepth);
            _nodes[idx].One = GetEmptyNode(new BoundingBox2F(x + wm, y + hm, z, w), nextDepth);
            _nodes[idx].HaveSubNode = true;
        }

        private (bool, bool, bool, bool) GetInsertNode(BoundingBox2F pRect, int idx)
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

        public void Add(BoundingBox2F rect, T item) { Add(rect, item, 0); }

        private void Add(BoundingBox2F rect, T item, int node)
        {
            if (_nodes[node].HaveSubNode)
            {
                var (rt, lt, ld, rd) = GetInsertNode(rect, node);
                if (rt) Add(rect, item, _nodes[node].One);
                if (lt) Add(rect, item, _nodes[node].Two);
                if (ld) Add(rect, item, _nodes[node].Three);
                if (rd) Add(rect, item, _nodes[node].Four);
                return;
            }

            _nodes[node].Data.Add((rect, item));
            if (_nodes[node].Data.Count > _maxItem && _nodes[node].Depth <= _maxDepth)
            {
                if (!_nodes[node].HaveSubNode)
                {
                    Split(node);
                }

                foreach (var (r, i) in _nodes[node].Data)
                {
                    var (rt, lt, ld, rd) = GetInsertNode(r, node);
                    if (rt) Add(r, i, _nodes[node].One);
                    if (lt) Add(r, i, _nodes[node].Two);
                    if (ld) Add(r, i, _nodes[node].Three);
                    if (rd) Add(r, i, _nodes[node].Four);
                }

                _nodes[node].Data.Clear();
            }
        }

        public T[] Retrieve(BoundingBox2F rect)
        {
            var set = new HashSet<T>();
            Retrieve(rect, 0, set);
            return set.ToArray();
        }

        public void Retrieve(BoundingBox2F rect, List<T> result)
        {
            var set = new HashSet<T>();
            Retrieve(rect, 0, set);
            result.Clear();
            result.AddRange(set);
        }

        public void Retrieve(BoundingBox2F rect, HashSet<T> result) { Retrieve(rect, 0, result); }

        private void Retrieve(BoundingBox2F rect, int node, HashSet<T> set)
        {
            foreach (var (_, item) in _nodes[node].Data)
            {
                set.Add(item);
            }

            if (_nodes[node].HaveSubNode)
            {
                var (rt, lt, ld, rd) = GetInsertNode(rect, node);
                if (rt) Retrieve(rect, _nodes[node].One, set);
                if (lt) Retrieve(rect, _nodes[node].Two, set);
                if (ld) Retrieve(rect, _nodes[node].Three, set);
                if (rd) Retrieve(rect, _nodes[node].Four, set);
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