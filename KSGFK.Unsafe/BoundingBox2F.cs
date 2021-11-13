using System;
using System.Numerics;

namespace KSGFK.Unsafe
{
    public readonly struct BoundingBox2F : IEquatable<BoundingBox2F>
    {
        public readonly float Left;
        public readonly float Down;
        public readonly float Right;
        public readonly float Up;

        public float Width => Right - Left;
        public float Height => Up - Down;

        public BoundingBox2F(float left, float down, float right, float up)
        {
            Left = left;
            Down = down;
            Right = right;
            Up = up;
        }

        public bool IsCross(BoundingBox2F o)
        {
            var left = MathF.Max(Left, o.Left);
            var down = MathF.Max(Down, o.Down);
            var right = MathF.Min(Right, o.Right);
            var up = MathF.Min(Up, o.Up);
            return left < right && down < up;
        }

        public bool Contains(BoundingBox2F o) { return o.Left >= Left && o.Right <= Right && o.Up <= Up && o.Down >= Down; }

        public override string ToString() { return $"[{new Vector2(Left, Down)},{new Vector2(Right, Up)}]"; }

        public bool Equals(BoundingBox2F other)
        {
            return MathF.Abs(Left - other.Left) < 0.000001f &&
                   MathF.Abs(Down - other.Down) < 0.000001f &&
                   MathF.Abs(Right - other.Right) < 0.000001f &&
                   MathF.Abs(Up - other.Up) < 0.000001f;
        }

        public override bool Equals(object obj) { return obj is BoundingBox2F other && Equals(other); }

        public override int GetHashCode() { return HashCode.Combine(Left, Down, Right, Up); }

        public static bool operator ==(BoundingBox2F l, BoundingBox2F r) { return l.Equals(r); }

        public static bool operator !=(BoundingBox2F l, BoundingBox2F r) { return !l.Equals(r); }
    }
}