using System;
using System.Collections.Generic;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using KSGFK.Collections;

namespace KSGFK.Unsafe.Benchmark
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class BenchmarkPriorityQueue
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

        private PriorityQueue<Vector4> _priority;
        private NativePriorityQueue<Vector4> _native;
        private Vector4[] _data;
        private CompareVec4 _cmp;

        [GlobalSetup]
        public void Setup()
        {
            _cmp = new CompareVec4();
            _priority = new PriorityQueue<Vector4>(1000000, _cmp);
            _native = new NativePriorityQueue<Vector4>(0, 1000000);
            _data = new Vector4[1000000];
            var rand = new Random();
            for (int i = 0; i < _data.Length; i++)
            {
                _data[i] = new Vector4(
                    rand.Next(1000000),
                    rand.Next(1000000),
                    rand.Next(1000000),
                    rand.Next(1000000));
            }
        }

        [Benchmark]
        public void ManagedPriorityQueue()
        {
            foreach (var vec4 in _data)
            {
                _priority.Enqueue(vec4);
            }

            while (!_priority.IsEmpty)
            {
                _priority.Dequeue();
            }
        }

        [Benchmark]
        public void NativePriorityQueue()
        {
            foreach (var vec4 in _data)
            {
                _native.Enqueue(vec4, _cmp);
            }

            while (!_native.IsEmpty)
            {
                _native.Dequeue(_cmp);
            }
        }

        [GlobalCleanup]
        public void Clean() { _native.Dispose(); }
    }
}