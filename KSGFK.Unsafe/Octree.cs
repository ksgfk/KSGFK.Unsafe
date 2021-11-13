using System;
using System.Collections.Generic;
using System.Numerics;

namespace KSGFK.Unsafe
{
    public interface IProvideCollider3F
    {
        /// <summary>
        /// 包围盒
        /// </summary>
        BoundingBox3F BoundingBox { get; }

        /// <summary>
        /// 图元求交
        /// </summary>
        /// <param name="ray">光线</param>
        /// <param name="u">重心坐标U分量</param>
        /// <param name="v">重心坐标V分量</param>
        /// <param name="t">射线起点到交点距离</param>
        /// <returns>是否有交点</returns>
        bool RayIntersect(in Ray3F ray, out float u, out float v, out float t);
    }

    /// <summary>
    /// 八叉树 跨节点图元存在叶节点内，其他节点不存图元
    /// <para>如果树到达最大深度，则忽略单个节点最大图元数量的限制</para>
    /// <para>0号节点默认空，表示空指针，根节点是1号节点</para>
    /// <para>插入和光线求交非递归，但求交还是有爆栈风险，若爆栈需要调整<see cref="Octree{T}.StackSize"/></para>
    /// </summary>
    public class Octree<T> where T : IProvideCollider3F
    {
        //256基本60W个节点不会爆栈了，再高没试过（
        public const int StackSize = 256;

        private struct NodeData
        {
            public T Shape;
        }

        private class Node
        {
            public BoundingBox3F Box { get; internal set; }
            public List<NodeData> Data { get; }
            internal int Child { get; set; } //只存8个子节点中第一个子节点索引，因为子节点是连续分配的

            public Node(in BoundingBox3F box)
            {
                Box = box;
                Data = new List<NodeData>(0);
            }
        }

        private readonly List<T> _shapes;
        private readonly List<Node> _tree;
        private readonly int _maxDepth;
        private readonly int _maxCount;
        private int _nowDepth;
        private int _leafCount;
        private int _nodeCount;
        private int _primitiveCount;
        private bool _isBuild;

        /// <summary>
        /// 节点数量
        /// </summary>
        public int NodeCount => _nodeCount;
        /// <summary>
        /// 当前深度
        /// </summary>
        public int NowDepth => _nowDepth;
        /// <summary>
        /// 叶节点数量
        /// </summary>
        public int LeafCount => _leafCount;
        /// <summary>
        /// 图元数量
        /// </summary>
        public int PrimitiveCount => _primitiveCount;
        /// <summary>
        /// 单节点最大图元数量
        /// </summary>
        public int MaxCount => _maxCount;
        /// <summary>
        /// 树最大深度
        /// </summary>
        public int MaxDepth => _maxDepth;

        public Octree(int maxDepth = 10, int maxCount = 16)
        {
            _maxDepth = maxDepth;
            _maxCount = maxCount;
            _tree = new List<Node>();
            _shapes = new List<T>();
            Init();
        }

        private void Init() { _tree.Add(null); }

        public void Clear()
        {
            _tree.Clear();
            Init();
            _nowDepth = 0;
            _leafCount = 0;
            _nodeCount = 0;
            _isBuild = false;
        }

        public void Add(T shape)
        {
            _shapes.Add(shape);
            if (_tree.Count <= 1)
            {
                _tree.Add(new Node(shape.BoundingBox)); //插入根节点
            }
            else
            {
                _tree[1].Box = _tree[1].Box.ExpandBy(shape.BoundingBox); //扩大根节点包围盒
            }
        }

        public void Build()
        {
            if (_isBuild)
            {
                return;
            }
            foreach (var shape in _shapes)
            {
                Build(shape);
            }
            _shapes.Clear();
            _shapes.TrimExcess();
            _isBuild = true;
            //Debug.LogInfo($"Octree deep:{NowDepth}");
            //Debug.LogInfo($"Octree node:{NodeCount}");
            //Debug.LogInfo($"Octree leaf:{LeafCount}");
            //Debug.LogInfo($"Octree primitive count:{PrimitiveCount}");
        }

        private void Build(T shape)
        {
            var box = shape.BoundingBox;
            var q = new Queue<(int, int)>(StackSize); //存节点编号和这个节点的深度
            q.Enqueue((1, 1)); //从根节点出发
            while (q.Count != 0)
            {
                var (ptr, dep) = q.Dequeue();
                if (!_tree[ptr].Box.Overlaps(in box))
                {
                    continue;
                }
                _nowDepth = Math.Max(_nowDepth, dep); //调整当前树深度
                if (_tree[ptr].Child == 0) //是叶节点就插入
                {
                    _tree[ptr].Data.Add(new NodeData { Shape = shape });
                    if (_tree[ptr].Data.Count < _maxCount) //检查叶节点是否需要分裂
                    {
                        continue;
                    }
                    if (dep >= _maxDepth)
                    {
                        continue;
                    }
                    _nodeCount += 8;
                    _leafCount += 7;
                    _tree[ptr].Child = _tree.Count;
                    for (var i = 0; i < 8; i++) //计算八个子节点包围盒并插入树中
                    {
                        Vector3 minPoint = default;
                        Vector3 maxPoint = default;
                        var center = _tree[ptr].Box.Center;
                        var corner = _tree[ptr].Box.GetCorner(i);
                        minPoint.X = MathF.Min(center.X, corner.X);
                        minPoint.Y = MathF.Min(center.Y, corner.Y);
                        minPoint.Z = MathF.Min(center.Z, corner.Z);
                        maxPoint.X = MathF.Max(center.X, corner.X);
                        maxPoint.Y = MathF.Max(center.Y, corner.Y);
                        maxPoint.Z = MathF.Max(center.Z, corner.Z);

                        var childBox = new BoundingBox3F(minPoint, maxPoint);
                        if (!childBox.IsValid)
                        {
                            throw new ArgumentException();
                        }
                        _tree.Add(new Node(childBox));
                    }
                    foreach (var data in _tree[ptr].Data) //将节点内图元和所有子节点测试是否覆盖
                    {
                        for (var chPtr = _tree[ptr].Child; chPtr < _tree[ptr].Child + 8; chPtr++)
                        {
                            if (_tree[chPtr].Box.Overlaps(data.Shape.BoundingBox))
                            {
                                _tree[chPtr].Data.Add(data);
                            }
                        }
                    }
                    _tree[ptr].Data.Clear(); //清理节点数据
                    _tree[ptr].Data.TrimExcess();
                }
                else
                {
                    for (var i = 0; i < 8; i++) //不是叶节点则递归
                    {
                        q.Enqueue((_tree[ptr].Child + i, dep + 1));
                    }
                }
            }
            _primitiveCount++;
        }

        public bool RayIntersect(in Ray3F ray, bool shadow, out IProvideCollider3F p, out float u, out float v, out float t)
        {
            p = default;
            u = default;
            v = default;
            t = default;
            if (_tree.Count <= 1)
            {
                return false;
            }
            if (!_tree[1].Box.RayIntersect(in ray)) //不根节点相交
            {
                return false;
            }
            var q = new SpanQueue<int>(stackalloc int[StackSize]);
            q.Enqueue(1);
            var nowRay = ray;
            var isHit = false;
            while (q.Count > 0)
            {
                var ptr = q.Peek();
                q.Dequeue();
                var node = _tree[ptr];
                if (node.Child == 0) //已经是叶节点了
                {
                    for (var i = 0; i < node.Data.Count; i++) //图元精确求交
                    {
                        var data = node.Data[i];
                        var shape = data.Shape;
                        if (shape.RayIntersect(in nowRay, out var tU, out var tV, out var tT))
                        {
                            if (shadow)
                            {
                                return true;
                            }
                            isHit = true;
                            nowRay = nowRay.SetMaxT(tT);
                            p = shape;
                            u = tU;
                            v = tV;
                            t = tT;
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < 8; i++) //不是叶节点，递归
                    {
                        var child = node.Child + i;
                        if (_tree[child].Box.RayIntersect(in nowRay))
                        {
                            q.Enqueue(child);
                        }
                    }
                }
            }
            return isHit;
        }
    }
}