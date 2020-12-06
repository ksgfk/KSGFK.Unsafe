using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace KSGFK.Unsafe.Benchmark
{
    /*
     |   Method |         aabbs |             Mean |         Error |        StdDev |
     |--------- |-------------- |-----------------:|--------------:|--------------:|
     |  ForEach | AABB2D[10000] | 1,439,628.987 us | 7,874.5717 us | 7,365.8793 us |
     | QuadTree | AABB2D[10000] |   536,990.871 us | 1,326.1608 us | 1,175.6070 us |
     |  ForEach |  AABB2D[1000] |    15,035.354 us |    95.4740 us |    84.6352 us |
     | QuadTree |  AABB2D[1000] |     6,135.707 us |    62.4634 us |    55.3722 us |
     |  ForEach |   AABB2D[100] |       151.944 us |     0.3168 us |     0.2809 us |
     | QuadTree |   AABB2D[100] |       164.132 us |     1.3974 us |     1.3071 us |
     |  ForEach |    AABB2D[10] |         1.267 us |     0.0064 us |     0.0054 us |
     | QuadTree |    AABB2D[10] |         9.285 us |     0.0996 us |     0.0932 us |
     */
    public class BenchmarkQuadTree
    {
        [Benchmark]
        [ArgumentsSource(nameof(GetTestData))]
        public void ForEach(AABB2D[] aabbs)
        {
            foreach (var aabb in aabbs)
            {
                var cross = new List<int>();
                for (var i = 0; i < aabbs.Length; i++)
                {
                    if (aabbs[i].IsCross(aabb))
                    {
                        cross.Add(i);
                    }
                }

                cross.Clear();
                cross.TrimExcess();
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetTestData))]
        public void QuadTree(AABB2D[] aabbs)
        {
            int depth;
            if (aabbs.Length <= 128)
            {
                depth = 2;
            }
            else if (aabbs.Length <= 1024)
            {
                depth = 3;
            }
            else
            {
                depth = 4;
            }

            var q = new QuadTree<int>(new AABB2D(-10, 10, 10, -10), depth);
            for (var i = 1; i < aabbs.Length; i++)
            {
                q.Add(aabbs[i], i);
            }

            foreach (var aabb in aabbs)
            {
                var testRes = new List<int>();
                q.CollisionTest(aabb, testRes);
                testRes.Clear();
                testRes.TrimExcess();
            }
        }

        public IEnumerable<AABB2D[]> GetTestData()
        {
            yield return GenArr(10);
            yield return GenArr(100);
            yield return GenArr(1000);
            yield return GenArr(10000);
        }

        public AABB2D[] GenArr(int cnt)
        {
            var rand = new Random();
            var data = new AABB2D[cnt];
            for (var i = 0; i < cnt; i++)
            {
                var left = (float) rand.NextDouble() * 19 - 10;
                var top = (float) rand.NextDouble() * 19 - 9;
                var width = (float) rand.NextDouble() + 1;
                var height = (float) rand.NextDouble() + 1;
                data[i] = new AABB2D(left, top, left + width, top - height);
            }

            return data;
        }
    }
}