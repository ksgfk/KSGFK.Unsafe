using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace KSGFK
{
    /// <summary>
    /// 表示一个点
    /// </summary>
    public readonly struct Point3F
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        // 异常低的性能
        // [Obsolete]
        // public float this[int i]
        // {
        //     [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        //     get
        //     {
        //         return i switch
        //         {
        //             0 => X,
        //             1 => Y,
        //             2 => Z,
        //             _ => throw new IndexOutOfRangeException()
        //         };
        //     }
        // }

        public Point3F(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3F(float value) : this(value, value, value) { }

        public Point3F(Span<float> span) : this(span[0], span[1], span[2]) { }

        public Point3F(Vector3 v) : this(v.X, v.Y, v.Z) { }

        /// <summary>
        /// 所有分量是否小于另一个点
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AreAllLessThen(in Point3F o) { return X < o.X && Y < o.Y && Z < o.Z; }

        /// <summary>
        /// 所有分量是否大于另一个点
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AreAllMoreThen(in Point3F o) { return X > o.X && Y > o.Y && Z > o.Z; }

        /// <summary>
        /// 所有分量是否小于等于另一个点
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AreAllLessThenOrEqual(in Point3F o) { return X <= o.X && Y <= o.Y && Z <= o.Z; }

        /// <summary>
        /// 所有分量是否大于等于另一个点
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AreAllMoreThenOrEqual(in Point3F o) { return X >= o.X && Y >= o.Y && Z >= o.Z; }

        /// <summary>
        /// 所有分量是否等于另一个点
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AreAllEqual(in Point3F o)
        {
            return MathF.Abs(X - o.X) < float.Epsilon &&
                   MathF.Abs(Y - o.Y) < float.Epsilon &&
                   MathF.Abs(Z - o.Z) < float.Epsilon;
        }

        /// <summary>
        /// 合并两个点，每个分量取最小值
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point3F MergeMin(in Point3F p) { return new Point3F(MathF.Min(X, p.X), MathF.Min(Y, p.Y), MathF.Min(Z, p.Z)); }

        /// <summary>
        /// 合并两个点，每个分量取最大值
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point3F MergeMax(in Point3F p) { return new Point3F(MathF.Max(X, p.X), MathF.Max(Y, p.Y), MathF.Max(Z, p.Z)); }

        public override string ToString() { return $"<{X}, {Y}, {Z}>"; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector3(Point3F p) { return new Point3F(p.X, p.Y, p.Z); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Point3F(Vector3 p) { return new Point3F(p.X, p.Y, p.Z); }
    }
}