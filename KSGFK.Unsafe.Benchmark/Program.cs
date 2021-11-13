using BenchmarkDotNet.Running;

namespace KSGFK.Unsafe.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<BenchmarkAvlSetInsert>();
            BenchmarkRunner.Run<BenchmarkAvlSetFind>();
            BenchmarkRunner.Run<BenchmarkAvlSetRemove>();
        }
    }
}