using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace KSGFK.Unsafe.Benchmark
{
    /*
|        Method |               Mean |           Error |            StdDev |             Median |
|-------------- |-------------------:|----------------:|------------------:|-------------------:|
|    RBTreeFind |    612,688.2212 ns |     786.6770 ns |       656.9109 ns |    612,724.1211 ns |
|   AVLTreeFind |    693,850.8440 ns |   3,753.6369 ns |     3,327.5013 ns |    694,199.3164 ns |

|    RBTreeFind |  7,052,188.0729 ns |  38,908.7416 ns |    36,395.2610 ns |  7,067,969.5312 ns |
|   AVLTreeFind | 10,358,667.1875 ns |  60,278.2930 ns |    56,384.3527 ns | 10,341,128.1250 ns |

|  RBTreeRemove |          1.5951 ns |       0.0114 ns |         0.0107 ns |          1.6000 ns |
| AVLTreeRemove |          0.2435 ns |       0.0093 ns |         0.0087 ns |          0.2433 ns |

|  RBTreeRemove |          3.5850 ns |       0.0345 ns |         0.0323 ns |          3.5908 ns |
| AVLTreeRemove |          0.2516 ns |       0.0056 ns |         0.0047 ns |          0.2525 ns |

|  RBTreeInsert | 28,879,292.0703 ns | 507,819.0912 ns | 1,328,874.0517 ns | 28,288,934.3750 ns |
| AVLTreeInsert | 30,122,833.3333 ns | 196,161.6790 ns |   183,489.7562 ns | 30,132,153.1250 ns |

|  RBTreeInsert |  1,532,273.4096 ns |   6,935.8903 ns |     6,148.4861 ns |  1,532,908.2031 ns |
| AVLTreeInsert |  1,547,391.7318 ns |   5,301.8065 ns |     4,959.3131 ns |  1,546,432.4219 ns |
     */
    
    public class BenchmarkAvlSet
    {
        [Benchmark]
        [ArgumentsSource(nameof(GetData))]
        public void RBTreeInsert(int[] data)
        {
            var set = new SortedSet<int>();
            foreach (var i in data)
            {
                set.Add(i);
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetData))]
        public void AVLTreeInsert(int[] data)
        {
            var set = new AvlSet<int>();
            foreach (var i in data)
            {
                set.Add(i);
            }
        }

        public IEnumerable<int[]> GetData()
        {
            yield return Gen(10000);
            yield return Gen(100000);
        }

        public int[] Gen(int count)
        {
            var raw = Enumerable.Range(1, count);
            var datas = raw.ToArray();
            var rand = new Random();
            for (var i = datas.Length - 1; i > 0; i--)
            {
                var value = datas[i];
                var randomIndex = rand.Next(0, i);
                datas[i] = datas[randomIndex];
                datas[randomIndex] = value;
            }

            return datas;
        }

        public SortedSet<int> GenRbt(int count) { return new SortedSet<int>(Gen(count)); }

        public AvlSet<int> GenAvl(int count)
        {
            var a = Gen(count);
            var avl = new AvlSet<int>();
            foreach (var i in a)
            {
                avl.Add(i);
            }

            return avl;
        }

        public IEnumerable<SortedSet<int>> GetRbt()
        {
            yield return GenRbt(10000);
            yield return GenRbt(100000);
        }

        public IEnumerable<AvlSet<int>> GetAvl()
        {
            yield return GenAvl(10000);
            yield return GenAvl(100000);
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetRbt))]
        public void RBTreeFind(SortedSet<int> set)
        {
            var count = set.Count;
            for (var i = 1; i <= count; i++)
            {
                _ = set.Contains(i);
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetAvl))]
        public void AVLTreeFind(AvlSet<int> set)
        {
            var count = set.Count;
            for (var i = 1; i <= count; i++)
            {
                _ = set.Contains(i);
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetRbt))]
        public void RBTreeRemove(SortedSet<int> set)
        {
            while (set.Count > 0)
            {
                set.Remove(set.Min);
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetAvl))]
        public void AVLTreeRemove(AvlSet<int> set)
        {
            while (set.Count > 0)
            {
                set.Remove(set.Min);
            }
        }
    }
}