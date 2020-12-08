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
        public void TestNew()
        {
            var rand = new Random();
            const int cnt = 500;
            var data = new Aabb2D[cnt];
            for (var i = 0; i < cnt; i++)
            {
                var x = (float) rand.NextDouble() * 20 - 10;
                var y = (float) rand.NextDouble() * 20 - 10;
                var w = (float) rand.NextDouble() * 4 + 1;
                var h = (float) rand.NextDouble() * 4 + 1;
                data[i] = new Aabb2D(x, y, x + w, y + h);
            }

            Console.WriteLine($"test:{data[0]}");
            Console.WriteLine("-----For-----");
            var crossA = new List<int>();
            for (var i = 1; i < cnt; i++)
            {
                if (data[i].IsCross(data[0]))
                {
                    crossA.Add(i);
                    Console.WriteLine($"{i}:{data[i]}");
                }
            }

            Console.WriteLine("-----QuadTree-----");
            var q = new QuadTree<int>(new Aabb2D(-10, -10, 10, 10));
            for (var i = 1; i < cnt; i++)
            {
                q.Add(data[i], i);
            }

            var res = q.Retrieve(data[0]);
            var crossB = new List<int>();
            for (var i = 0; i < res.Length; i++)
            {
                if (data[res[i]].IsCross(data[0]))
                {
                    crossB.Add(res[i]);
                    Console.WriteLine($"{res[i]}:{data[i]}");
                }
            }

            Assert.True(crossA.ToHashSet().SetEquals(crossB.ToHashSet()));
        }
    }
}