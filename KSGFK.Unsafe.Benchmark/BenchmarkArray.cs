using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace KSGFK.Unsafe.Benchmark
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class BenchmarkArray
    {
        private Matrix4x4[] _standardArr;
        private NativeArray<Matrix4x4> _nativeArray;

        [GlobalSetup]
        public void Setup()
        {
            _standardArr = new Matrix4x4[10000000];
            _nativeArray = new NativeArray<Matrix4x4>(10000000, 0);
        }

        [Benchmark]
        public void StandardArray()
        {
            for (var i = 0; i < _standardArr.Length; i++)
            {
                _standardArr[i] = Matrix4x4.Identity;
            }
        }

        [Benchmark]
        public void NativeArray()
        {
            for (var i = 0; i < _nativeArray.Count; i++)
            {
                _nativeArray[i] = Matrix4x4.Identity;
            }
        }

        [GlobalCleanup]
        public void Clean() { _nativeArray.Dispose(); }
    }
}