using BenchmarkDotNet.Running;

namespace KSGFK.Unsafe.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<BenchmarkArray>();
            BenchmarkRunner.Run<BenchmarkPriorityQueue>();
            BenchmarkRunner.Run<BenchmarkList>();
            BenchmarkRunner.Run<BenchmarkQuickSort>();
        }
    }
}