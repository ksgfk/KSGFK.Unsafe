using System;
using System.Numerics;

namespace KSGFK.Unsafe
{
    public readonly struct AABB2D
    {
        public readonly float Left;
        public readonly float Top;
        public readonly float Right;
        public readonly float Down;

        public float Width => Right - Left;
        public float Height => Top - Down;
        public Vector2 LeftTop => new Vector2(Left, Top);
        public Vector2 RightDown => new Vector2(Right, Down);

        public AABB2D(float left, float top, float right, float down)
        {
            if (left > right) throw new ArgumentException();
            if (top < down) throw new ArgumentException();
            Left = left;
            Top = top;
            Right = right;
            Down = down;
        }

        public AABB2D(Vector2 leftTop, Vector2 rightDown) : this(leftTop.X, leftTop.Y, rightDown.X, rightDown.Y) { }

        public bool Contains(AABB2D other)
        {
            return other.Left >= Left && other.Right <= Right && other.Top <= Top && other.Down >= Down;
        }

        public bool IsCross(AABB2D other)
        {
            var left = MathF.Max(Left, other.Left);
            var top = MathF.Min(Top, other.Top);
            var right = MathF.Min(Right, other.Right);
            var down = MathF.Max(Down, other.Down);
            return left < right && top > down;
        }

        public override string ToString() { return $"[{LeftTop},{RightDown}]"; }
    }
}