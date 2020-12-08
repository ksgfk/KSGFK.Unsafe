using System;
using System.Numerics;

namespace KSGFK.Unsafe
{
    public readonly struct Aabb2D : IEquatable<Aabb2D>
    {
        public readonly float Left;
        public readonly float Down;
        public readonly float Right;
        public readonly float Up;

        public float Width => Right - Left;
        public float Height => Up - Down;

        public Aabb2D(float left, float down, float right, float up)
        {
            Left = left;
            Down = down;
            Right = right;
            Up = up;
        }

        public bool IsCross(Aabb2D o)
        {
            var left = MathF.Max(Left, o.Left);
            var down = MathF.Max(Down, o.Down);
            var right = MathF.Min(Right, o.Right);
            var up = MathF.Min(Up, o.Up);
            return left < right && down < up;
        }

        public bool Contains(Aabb2D o) { return o.Left >= Left && o.Right <= Right && o.Up <= Up && o.Down >= Down; }

        public override string ToString() { return $"[{new Vector2(Left, Down)},{new Vector2(Right, Up)}]"; }

        public bool Equals(Aabb2D other)
        {
            return MathF.Abs(Left - other.Left) < 0.000001f &&
                   MathF.Abs(Down - other.Down) < 0.000001f &&
                   MathF.Abs(Right - other.Right) < 0.000001f &&
                   MathF.Abs(Up - other.Up) < 0.000001f;
        }

        public override bool Equals(object obj) { return obj is Aabb2D other && Equals(other); }

        public override int GetHashCode() { return HashCode.Combine(Left, Down, Right, Up); }

        public static bool operator ==(Aabb2D l, Aabb2D r) { return l.Equals(r); }

        public static bool operator !=(Aabb2D l, Aabb2D r) { return !l.Equals(r); }
    }
}