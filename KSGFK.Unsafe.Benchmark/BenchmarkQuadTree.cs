using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace KSGFK.Unsafe.Benchmark
{
    /*
|   Method |         aabbs |         Mean |     Error |    StdDev |
|--------- |-------------- |-------------:|----------:|----------:|
|  ForEach | Aabb2D[10000] | 1,419.016 ms | 3.3656 ms | 3.1481 ms |
| QuadTree | Aabb2D[10000] |   883.890 ms | 4.7863 ms | 4.4771 ms |
|  ForEach |  Aabb2D[1000] |    14.538 ms | 0.0467 ms | 0.0436 ms |
| QuadTree |  Aabb2D[1000] |     9.777 ms | 0.0497 ms | 0.0464 ms |
     */
    public class BenchmarkQuadTree
    {
        private List<int> _feRes = new List<int>();
        private List<int> _tree = new List<int>();
        private List<int> _may = new List<int>();

        [Benchmark]
        [ArgumentsSource(nameof(GetTestData))]
        public void ForEach(Aabb2D[] aabbs)
        {
            _feRes.Clear();
            foreach (var aabb in aabbs)
            {
                for (var i = 0; i < aabbs.Length; i++)
                {
                    if (aabbs[i].IsCross(aabb))
                    {
                        _feRes.Add(i);
                    }
                }
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetTestData))]
        public void QuadTree(Aabb2D[] aabbs)
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

            var q = new QuadTree<int>(new Aabb2D(-10, -10, 10, 10), maxDepth: depth);
            for (var i = 1; i < aabbs.Length; i++)
            {
                q.Add(aabbs[i], i);
            }

            _tree.Clear();
            foreach (var t in aabbs)
            {
                var aabb = t;
                q.Retrieve(aabb, _may);
                foreach (var t1 in _may)
                {
                    if (aabbs[t1].IsCross(t))
                    {
                        _tree.Add(t1);
                    }
                }
            }
        }

        public IEnumerable<Aabb2D[]> GetTestData()
        {
            yield return GenArr(1000);
            yield return GenArr(10000);
        }

        public Aabb2D[] GenArr(int cnt)
        {
            var rand = new Random();
            var data = new Aabb2D[cnt];
            for (var i = 0; i < cnt; i++)
            {
                var x = (float) rand.NextDouble() * 20 - 10;
                var y = (float) rand.NextDouble() * 20 - 10;
                var w = (float) rand.NextDouble() * 3 + 1;
                var h = (float) rand.NextDouble() * 3 + 1;
                data[i] = new Aabb2D(x, y, x + w, y + h);
            }

            return data;
        }
    }
}