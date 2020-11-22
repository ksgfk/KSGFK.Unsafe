using System;
using System.Collections.Generic;
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

        private List<Vector4> _managedList;
        private NativeList<Vector4> _nativeList;
        private Vector4[] _data;
        private int _cnt = 1000;
        private CompareVec4 _cmp;

        [GlobalSetup]
        public void Setup()
        {
            _cmp = new CompareVec4();
            _data = new Vector4[_cnt];
            _managedList = new List<Vector4>(_cnt);
            _nativeList = new NativeList<Vector4>(_cnt, 1);
            var rand = new Random();
            for (var i = 0; i < _data.Length; i++)
            {
                _data[i] = new Vector4(
                    rand.Next(1000000),
                    rand.Next(1000000),
                    rand.Next(1000000),
                    rand.Next(1000000));
            }

            foreach (var d in _data)
            {
                _managedList.Add(d);
            }

            foreach (var d in _data)
            {
                _nativeList.Add(d);
            }
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public void ManagedList() { _managedList.Sort(_cmp); }

        [Benchmark(OperationsPerInvoke = 1)]
        public void NativeList() { _nativeList.Sort(_cmp); }

        [GlobalCleanup]
        public void Cleanup() { _nativeList.Dispose(); }
    }
}