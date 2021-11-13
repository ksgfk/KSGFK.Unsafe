using System;
using NUnit.Framework;

namespace KSGFK.Unsafe.Test
{
    public class TestBoundingBox2F
    {
        private (bool, bool, bool, bool) GetInsertNode(BoundingBox2F pRect, BoundingBox2F bound)
        {
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

        [Test]
        public void Test()
        {
            var bound = new BoundingBox2F(0, 0, 40, 40);
            Console.WriteLine(bound.Width);
            Console.WriteLine(bound.Height);
            var wm = bound.Width / 2;
            var hm = bound.Height / 2;
            var x = bound.Left;
            var y = bound.Down;
            var z = bound.Right;
            var w = bound.Up;
            var one = new BoundingBox2F(x + wm, y + hm, z, w);
            var two = new BoundingBox2F(x, y + hm, z - wm, w);
            var three = new BoundingBox2F(x, y, z - wm, w - hm);
            var four = new BoundingBox2F(x + wm, y, z, w - hm);
            Console.WriteLine(one);
            Console.WriteLine(two);
            Console.WriteLine(three);
            Console.WriteLine(four);

            var (m, n, o, p) = GetInsertNode(new BoundingBox2F(30, 30, 31, 31), bound);
            Assert.True(m);
            Assert.False(n);
            Assert.False(o);
            Assert.False(p);
            (m, n, o, p) = GetInsertNode(new BoundingBox2F(1, 21, 31, 31), bound);
            Assert.True(m);
            Assert.True(n);
            Assert.False(o);
            Assert.False(p);
            (m, n, o, p) = GetInsertNode(new BoundingBox2F(1, 1, 31, 31), bound);
            Assert.True(m);
            Assert.True(n);
            Assert.True(o);
            Assert.True(p);
            (m, n, o, p) = GetInsertNode(new BoundingBox2F(21, 1, 31, 31), bound);
            Assert.True(m);
            Assert.False(n);
            Assert.False(o);
            Assert.True(p);
            (m, n, o, p) = GetInsertNode(new BoundingBox2F(1, 1, 2, 2), bound);
            Assert.False(m);
            Assert.False(n);
            Assert.True(o);
            Assert.False(p);
            (m, n, o, p) = GetInsertNode(new BoundingBox2F(1, 1, 31, 2), bound);
            Assert.False(m);
            Assert.False(n);
            Assert.True(o);
            Assert.True(p);

            var a = new BoundingBox2F(0, 0, 5, 5);
            var b = new BoundingBox2F(1, 1, 4, 4);
            Assert.True(a.Contains(b));
            Assert.True(a.IsCross(b));
            var c = new BoundingBox2F(-1, 1, 2, 2);
            Assert.False(a.Contains(c));
            Assert.True(a.IsCross(c));
            var d = new BoundingBox2F(-4, -4, -2, -2);
            Assert.False(a.Contains(d));
            Assert.False(a.IsCross(d));
        }
    }
}