using System;
using KSGFK.Unsafe;
using NUnit.Framework;

namespace Test
{
    public class TestAABB
    {
        [Test]
        public void Test()
        {
            var nowSize = new AABB2D(-10, 10, 10, -10);
            var newNodeW = nowSize.Width / 2f;
            var newNodeH = nowSize.Height / 2f;
            var rt = new AABB2D(nowSize.Left + newNodeW, nowSize.Top, nowSize.Right, nowSize.Down + newNodeH);
            var lt = new AABB2D(nowSize.Left, nowSize.Top, nowSize.Right - newNodeW, nowSize.Down + newNodeH);
            var ld = new AABB2D(nowSize.Left, nowSize.Top - newNodeH, nowSize.Right - newNodeW, nowSize.Down);
            var rd = new AABB2D(nowSize.Left + newNodeW, nowSize.Top - newNodeH, nowSize.Right, nowSize.Down);
            Console.WriteLine(rt);
            Console.WriteLine(lt);
            Console.WriteLine(ld);
            Console.WriteLine(rd);

            var a = new AABB2D(0, 0, 5, -5);
            var b = new AABB2D(1, -1, 4, -4);
            Assert.True(a.Contains(b));
            Assert.True(a.IsCross(b));
            var c = new AABB2D(-1, 1, 2, -2);
            Assert.False(a.Contains(c));
            Assert.True(a.IsCross(c));
            var d = new AABB2D(-4, 4, -2, 0);
            Assert.False(a.Contains(d));
            Assert.False(a.IsCross(d));
        }
    }
}