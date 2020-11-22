using System.Collections.Generic;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace KSGFK.Unsafe.Benchmark
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class BenchmarkList
    {
        private List<Matrix4x4> _managedList;
        private NativeList<Matrix4x4> _nativeList;
        private int _cnt = 100;

        [GlobalSetup]
        public void Setup()
        {
            _managedList = new List<Matrix4x4>(_cnt);
            _nativeList = new NativeList<Matrix4x4>(_cnt, 1);
            for (var i = 0; i < _cnt; i++)
            {
                _nativeList.Add(Matrix4x4.Identity);
            }

            for (var i = 0; i < _cnt; i++)
            {
                _managedList.Add(Matrix4x4.Identity);
            }
        }

        [Benchmark]
        public void ManagedList()
        {
            for (var i = 0; i < _managedList.Count; i++)
            {
                _managedList[i] = Matrix4x4.Multiply(_managedList[i], -2);
            }
        }

        [Benchmark]
        public void NativeList()
        {
            foreach (ref var mat4 in _nativeList)
            {
                mat4 = Matrix4x4.Multiply(mat4, -2);
            }
        }

        [GlobalCleanup]
        public void Cleanup() { _nativeList.Dispose(); }
    }
}