using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace KSGFK.Unsafe.Benchmark
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class BenchmarkQuickSort
    {
        public class CompareVec4 : Comparer<Vector4>
        {
            public override int Compare(Vector4 x, Vector4 y)
            {
                var xlen = MathF.Sqrt(x.X * x.X + x.Y + x.Y + x.Z * x.Z + x.W + x.W);
                var ylen = MathF.Sqrt(y.X * y.X + y.Y + y.Y + y.Z * y.Z + y.W + y.W);
                return xlen < ylen ? -1 : Math.Abs(xlen - ylen) < 0.0000001f ? 0 : 1;
            }
        }

        private int _cnt = (int) 1e5;

        [Benchmark]
        [ArgumentsSource(nameof(ManagedData))]
        public void ManagedList(List<int> list) { list.Sort(); }

        public IEnumerable<List<int>> ManagedData()
        {
            var random = new Random();
            var datas = Enumerable.Range(1, _cnt).ToList();
            for (int i = datas.Count - 1; i > 0; i--)
            {
                var value = datas[i];
                var randomIndex = random.Next(0, i);
                datas[i] = datas[randomIndex];
                datas[randomIndex] = value;
            }

            yield return datas;
        }

        [Benchmark]
        [ArgumentsSource(nameof(UnsafeData))]
        public void NativeList(NativeList<int> list) { list.Sort(); }

        public IEnumerable<NativeList<int>> UnsafeData()
        {
            var random = new Random();
            var datas = Enumerable.Range(1, _cnt).ToArray();
            for (int i = datas.Length - 1; i > 0; i--)
            {
                var value = datas[i];
                var randomIndex = random.Next(0, i);
                datas[i] = datas[randomIndex];
                datas[randomIndex] = value;
            }

            var na = new NativeList<int>(1, _cnt);
            foreach (var t in datas)
            {
                na.Add(t);
            }

            yield return na;
        }
    }
}