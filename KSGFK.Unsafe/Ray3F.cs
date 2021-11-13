using System.Numerics;
using System.Runtime.CompilerServices;

namespace KSGFK
{
    /// <summary>
    /// 光线
    /// </summary>
    public readonly struct Ray3F
    {
        /// <summary>
        /// 起点
        /// </summary>
        public readonly Point3F O;

        /// <summary>
        /// 方向
        /// </summary>
        public readonly Vector3 D;

        /// <summary>
        /// 1/方向
        /// </summary>
        public readonly Vector3 InvD;

        /// <summary>
        /// 最小距离
        /// </summary>
        public readonly float MinT;

        /// <summary>
        /// 最大距离
        /// </summary>
        public readonly float MaxT;

        public Ray3F(Point3F o, Vector3 d, float minT = 0, float maxT = float.PositiveInfinity)
        {
            O = o;
            D = d;
            MinT = minT;
            MaxT = maxT;
            InvD = new Vector3(1.0f / d.X, 1.0f / d.Y, 1.0f / d.Z);
        }

        /// <summary>
        /// 计算碰撞点坐标
        /// </summary>
        /// <param name="t">距离</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point3F At(float t) { return O + t * D; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Ray3F SetMinT(float minT) { return new Ray3F(O, D, minT, MaxT); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Ray3F SetMaxT(float maxT) { return new Ray3F(O, D, MinT, maxT); }

        public override string ToString() { return $"<{O} -> {D}>"; }
    }
}