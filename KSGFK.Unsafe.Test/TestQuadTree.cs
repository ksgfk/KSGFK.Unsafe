using System;
using System.Collections.Generic;
using System.Linq;
using KSGFK.Unsafe;
using NUnit.Framework;

namespace Test
{
    public class TestQuadTree
    {
        [Test]
        public void Test()
        {
            var rand = new Random();
            const int cnt = 100;
            var data = new List<AABB2D>(cnt);
            for (var i = 0; i < 100; i++)
            {
                var left = (float) rand.NextDouble() * 19 - 10;
                var top = (float) rand.NextDouble() * 19 - 9;
                var width = (float) rand.NextDouble() * 3 + 1;
                var height = (float) rand.NextDouble() * 3 + 1;
                data.Add(new AABB2D(left, top, left + width, top - height));
            }

            Console.WriteLine($"test:{data[0]}");
            var cross = new List<int>(cnt - 1);
            for (var i = 1; i < 100; i++)
            {
                if (data[i].IsCross(data[0]))
                {
                    cross.Add(i);
                    Console.WriteLine($"{i}:{data[i]}");
                }
            }

            var q = new QuadTree<int>(new AABB2D(-10, 10, 10, -10), 8);
            for (var i = 1; i < 100; i++)
            {
                q.Add(data[i], i);
            }

            var testRes = new List<int>();
            q.CollisionTest(data[0], testRes);
            var set = cross.ToHashSet();
            var res = testRes.ToHashSet();
            Console.WriteLine(cross.Count);
            Console.WriteLine(testRes.Count);
            Console.WriteLine(set.Count);
            Console.WriteLine(res.Count);
            Console.WriteLine();
            foreach (var r in res)
            {
                Console.WriteLine(data[r]);
            }

            Assert.True(set.Count == res.Count);
            Assert.True(set.SetEquals(res));
        }
    }
}