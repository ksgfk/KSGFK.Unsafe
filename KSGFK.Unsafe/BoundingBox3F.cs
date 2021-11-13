using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace KSGFK
{
    /// <summary>
    /// 轴对齐包围盒，保存最小位置和最大位置
    /// </summary>
    public readonly struct BoundingBox3F
    {
        /// <summary>
        /// 最小点
        /// </summary>
        public readonly Point3F Min;

        /// <summary>
        /// 最大点
        /// </summary>
        public readonly Point3F Max;

        /// <summary>
        /// 是否合法，也就是最小点任何值都小于等于最大点
        /// </summary>
        public bool IsValid => Max.AreAllMoreThenOrEqual(in Min);

        /// <summary>
        /// 是不是一个点，也就是最小点等于最大点
        /// </summary>
        public bool IsPoint => Max.AreAllEqual(in Min);

        /// <summary>
        /// 是否存在体积，是不是一个二维包围盒
        /// </summary>
        public bool HasVolume => Max.AreAllMoreThen(in Min);

        /// <summary>
        /// 范围
        /// </summary>
        public Vector3 Extents => (Vector3)Max - Min;

        /// <summary>
        /// 表面积
        /// </summary>
        public float SurfaceArea
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var d = (Vector3)Max - Min;
                var result = 0.0f;
                result += d.Y * d.Z;
                result += d.X * d.Z;
                result += d.X * d.Y;
                return result * 2.0f;
            }
        }

        /// <summary>
        /// 中心点
        /// </summary>
        public Point3F Center
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ((Vector3)Min + Max) * 0.5f;
        }

        public BoundingBox3F(in Point3F min, in Point3F max)
        {
            Min = min;
            Max = max;
        }

        public BoundingBox3F(in Point3F p) : this(in p, in p) { }

        /// <summary>
        /// 检查点是否位于边界框上或包围盒内
        /// </summary>
        /// <param name="p">需要检测的点</param>
        /// <param name="strict">是否包含边缘</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in Point3F p, bool strict = false)
        {
            return strict
                ? p.AreAllMoreThen(in Min) && p.AreAllLessThen(in Max)
                : p.AreAllMoreThenOrEqual(in Min) && p.AreAllLessThenOrEqual(in Max);
        }

        /// <summary>
        /// 检查包围盒是否位于边界框上或包围盒内
        /// <para>如果一个包围盒不合法，它不应该被包含在任何合法的包围盒内</para>
        /// </summary>
        /// <param name="box">需要检测的包围盒</param>
        /// <param name="strict">是否包含边缘</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in BoundingBox3F box, bool strict = false)
        {
            return strict
                ? box.Min.AreAllMoreThen(in Min) && box.Max.AreAllLessThen(in Max)
                : box.Min.AreAllMoreThenOrEqual(in Min) && box.Max.AreAllLessThenOrEqual(in Max);
        }

        /// <summary>
        /// 是否覆盖包围盒
        /// </summary>
        /// <param name="box">需要检测的包围盒</param>
        /// <param name="strict">是否包含边缘</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(in BoundingBox3F box, bool strict = false)
        {
            return strict
                ? box.Min.AreAllLessThen(in Max) && box.Max.AreAllMoreThen(in Min)
                : box.Min.AreAllLessThenOrEqual(in Max) && box.Max.AreAllMoreThenOrEqual(in Min);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float GetDistanceSquaredResult(float p, float min, float max)
        {
            if (p < min)
            {
                return min - p;
            }
            if (p > max)
            {
                return p - max;
            }
            return 0.0f;
        }

        /// <summary>
        /// 离点最近的距离的平方
        /// </summary>
        /// <param name="p">目标点</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float DistanceSquared(in Point3F p)
        {
            var x = GetDistanceSquaredResult(p.X, Min.X, Max.X);
            var y = GetDistanceSquaredResult(p.Y, Min.Y, Max.Y);
            var z = GetDistanceSquaredResult(p.Z, Min.Z, Max.Z);
            var result = x * x + y * y + z * z;
            return result;
        }

        /// <summary>
        /// 离点最近的距离
        /// </summary>
        /// <param name="p">目标点</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Distance(in Point3F p) { return MathF.Sqrt(DistanceSquared(in p)); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetDistanceSquaredResult(float boxMin, float boxMax, float min, float max)
        {
            if (boxMax < min)
            {
                return min - boxMax;
            }
            if (boxMin > max)
            {
                return boxMin - max;
            }
            return 0.0f;
        }

        /// <summary>
        /// 离包围盒最近的距离的平方
        /// </summary>
        /// <param name="box">目标包围盒</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float DistanceSquared(in BoundingBox3F box)
        {
            var x = GetDistanceSquaredResult(box.Min.X, box.Max.X, Min.X, Max.X);
            var y = GetDistanceSquaredResult(box.Min.Y, box.Max.Y, Min.Y, Max.Y);
            var z = GetDistanceSquaredResult(box.Min.Z, box.Max.Z, Min.Z, Max.Z);
            var result = x * x + y * y + z * z;
            return result;
        }

        /// <summary>
        /// 离包围盒最近的距离
        /// </summary>
        /// <param name="box">目标包围盒</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Distance(in BoundingBox3F box) { return MathF.Sqrt(DistanceSquared(in box)); }

        /// <summary>
        /// 将包围盒扩大到包含点
        /// </summary>
        /// <param name="p">需要包围的点</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BoundingBox3F ExpandBy(in Point3F p)
        {
            var min = Min.MergeMin(in p);
            var max = Max.MergeMax(in p);
            return new BoundingBox3F(in min, in max);
        }

        /// <summary>
        /// 将包围盒扩大到包含包围盒
        /// </summary>
        /// <param name="box">需要包围的包围盒</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BoundingBox3F ExpandBy(in BoundingBox3F box)
        {
            var min = Min.MergeMin(in box.Min);
            var max = Max.MergeMax(in box.Max);
            return new BoundingBox3F(in min, in max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetCornerResult(int index, int i, float max, float min)
        {
            return (index & (1 << i)) != 0 ? max : min;
        }

        /// <summary>
        /// 获取包围盒顶点坐标
        /// </summary>
        /// <param name="index">顶点索引</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point3F GetCorner(int index)
        {
            return new Point3F(GetCornerResult(index, 0, Max.X, Min.X),
                GetCornerResult(index, 1, Max.Y, Min.Y),
                GetCornerResult(index, 2, Max.Z, Min.Z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap<T>(ref T l, ref T r)
        {
            var t = l;
            l = r;
            r = t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool GetResult(float o, float d, float invD, float min, float max, ref float nearT, ref float farT)
        {
            var origin = o;
            var minVal = min;
            var maxVal = max;
            if (d == 0)
            {
                if (origin < minVal || origin > maxVal)
                {
                    return false;
                }
            }
            else
            {
                var t1 = (minVal - origin) * invD;
                var t2 = (maxVal - origin) * invD;
                if (t1 > t2)
                {
                    Swap(ref t1, ref t2);
                }
                nearT = MathF.Max(t1, nearT);
                farT = MathF.Min(t2, farT);

                if (!(nearT <= farT))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 光线求交
        /// </summary>
        /// <param name="ray">光线</param>
        /// <returns>包围盒是否与光线相交</returns>
        public bool RayIntersect(in Ray3F ray)
        {
            var nearT = float.NegativeInfinity;
            var farT = float.PositiveInfinity;
            var rx = GetResult(ray.O.X, ray.D.X, ray.InvD.X, Min.X, Max.X, ref nearT, ref farT);
            if (!rx) return false;
            var ry = GetResult(ray.O.Y, ray.D.Y, ray.InvD.Y, Min.Y, Max.Y, ref nearT, ref farT);
            if (!ry) return false;
            var rz = GetResult(ray.O.Z, ray.D.Z, ray.InvD.Z, Min.Z, Max.Z, ref nearT, ref farT);
            if (!rz) return false;
            return ray.MinT <= farT && nearT <= ray.MaxT;
        }

        /// <summary>
        /// 光线求交
        /// </summary>
        /// <param name="ray">光线</param>
        /// <param name="nearT">最近的面</param>
        /// <param name="farT">最远的面</param>
        /// <returns>包围盒是否与光线相交</returns>
        public bool RayIntersect(in Ray3F ray, out float nearT, out float farT)
        {
            nearT = float.NegativeInfinity;
            farT = float.PositiveInfinity;
            var rx = GetResult(ray.O.X, ray.D.X, ray.InvD.X, Min.X, Max.X, ref nearT, ref farT);
            if (!rx) return false;
            var ry = GetResult(ray.O.Y, ray.D.Y, ray.InvD.Y, Min.Y, Max.Y, ref nearT, ref farT);
            if (!ry) return false;
            var rz = GetResult(ray.O.Z, ray.D.Z, ray.InvD.Z, Min.Z, Max.Z, ref nearT, ref farT);
            if (!rz) return false;
            return ray.MinT <= farT && nearT <= ray.MaxT;
        }
    }
}