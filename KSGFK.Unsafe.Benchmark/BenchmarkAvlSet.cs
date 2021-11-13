using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace KSGFK.Unsafe.Benchmark
{
    /*
    BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
    Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
    .NET SDK=6.0.100
      [Host]     : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
      DefaultJob : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT

    | Method            |      Mean |    Error |   StdDev |
    | ----------------- | --------: | -------: | -------: |
    | RBTreeInsert10W   |  27.59 ms | 0.531 ms | 0.545 ms |
    | RBTreeInsert100W  | 665.73 ms | 7.123 ms | 6.663 ms |
    | AVLTreeInsert10W  |  26.69 ms | 0.271 ms | 0.254 ms |
    | AVLTreeInsert100W | 737.10 ms | 6.378 ms | 5.966 ms |
    */
    public class BenchmarkAvlSetInsert : BenchmarkAvlSet
    {
        [Benchmark]
        public void RBTreeInsert10W()
        {
            _10wSet = new SortedSet<int>();
            RBTreeInsert(_10w, _10wSet);
        }

        [Benchmark]
        public void RBTreeInsert100W()
        {
            _100wSet = new SortedSet<int>();
            RBTreeInsert(_100w, _100wSet);
        }

        [Benchmark]
        public void AVLTreeInsert10W()
        {
            _10wAvl = new AvlTree<int>();
            AVLTreeInsert(_10w, _10wAvl);
        }

        [Benchmark]
        public void AVLTreeInsert100W()
        {
            _100wAvl = new AvlTree<int>();
            AVLTreeInsert(_100w, _100wAvl);
        }
    }

    /*
    BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
    Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
    .NET SDK=6.0.100
    [Host]     : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
    DefaultJob : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT

    | Method           |        Mean |    Error |   StdDev |
    | ---------------- | ----------: | -------: | -------: |
    | RBTreeFind10W    |    65.45 ms | 0.262 ms | 0.219 ms |
    | RBTreeFind100W   |    97.58 ms | 0.549 ms | 0.513 ms |
    | RBTreeFind1000W  | 1,020.17 ms | 2.446 ms | 2.042 ms |
    | AVLTreeFind10W   |    53.00 ms | 0.102 ms | 0.090 ms |
    | AVLTreeFind100W  |    73.34 ms | 0.356 ms | 0.315 ms |
    | AVLTreeFind1000W |   644.45 ms | 3.578 ms | 3.347 ms |
    */
    public class BenchmarkAvlSetFind : BenchmarkAvlSet
    {
        private readonly int[] _rnd10W = new int[1_000_000];
        private readonly int[] _rnd100W = new int[1_000_000];
        private readonly int[] _rnd1000W = new int[10_000_000];

        public BenchmarkAvlSetFind() : base()
        {
            _10wSet = new SortedSet<int>();
            _100wSet = new SortedSet<int>();
            _10wAvl = new AvlTree<int>();
            _100wAvl = new AvlTree<int>();

            var rand = new Random();
            for (int i = 0; i < _rnd10W.Length; i++)
            {
                _rnd10W[i] = rand.Next(0, 100_000_000);
            }
            for (int i = 0; i < _rnd100W.Length; i++)
            {
                _rnd100W[i] = rand.Next(0, 100_000_000);
            }
            for (int i = 0; i < _rnd1000W.Length; i++)
            {
                _rnd1000W[i] = rand.Next(0, 100_000_000);
            }

            RBTreeInsert(_10w, _10wSet);
            RBTreeInsert(_100w, _100wSet);
            AVLTreeInsert(_10w, _10wAvl);
            AVLTreeInsert(_100w, _100wAvl);
        }

        [Benchmark]
        public void RBTreeFind10W()
        {
            RBTreeFind(_rnd10W, _10wSet);
        }

        [Benchmark]
        public void RBTreeFind100W()
        {
            RBTreeFind(_rnd100W, _100wSet);
        }

        [Benchmark]
        public void RBTreeFind1000W()
        {
            RBTreeFind(_rnd1000W, _100wSet);
        }

        [Benchmark]
        public void AVLTreeFind10W()
        {
            AVLTreeFind(_rnd10W, _10wAvl);
        }

        [Benchmark]
        public void AVLTreeFind100W()
        {
            AVLTreeFind(_rnd100W, _100wAvl);
        }

        [Benchmark]
        public void AVLTreeFind1000W()
        {
            AVLTreeFind(_rnd1000W, _100wAvl);
        }
    }

    /*
    BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
    Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
    .NET SDK=6.0.100
      [Host]     : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
      DefaultJob : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT

    | Method            |      Mean |     Error |    StdDev |
    | ----------------- | --------: | --------: | --------: |
    | RBTreeRemove10W   |  53.82 ms |  0.508 ms |  0.424 ms |
    | RBTreeRemove100W  | 905.52 ms | 12.177 ms | 10.168 ms |
    | AVLTreeRemove10W  |  33.67 ms |  0.451 ms |  0.400 ms |
    | AVLTreeRemove100W | 648.58 ms |  6.430 ms |  5.369 ms |
    */
    public class BenchmarkAvlSetRemove : BenchmarkAvlSet
    {
        private readonly int[] _1000w;
        private readonly SortedSet<int> _set;
        private readonly AvlTree<int> _avl;
        private readonly int[] _rnd10W = new int[1_00_000];
        private readonly int[] _rnd100W = new int[1_000_000];

        public BenchmarkAvlSetRemove()
        {
            _set = new SortedSet<int>();
            _avl = new AvlTree<int>();
            _1000w = Gen(10_000_000);
            var rand = new Random();
            for (int i = 0; i < _rnd10W.Length; i++)
            {
                _rnd10W[i] = rand.Next(0, _rnd10W.Length);
            }
            for (int i = 0; i < _rnd100W.Length; i++)
            {
                _rnd100W[i] = rand.Next(0, _rnd100W.Length);
            }

            RBTreeInsert(_1000w, _set);
            AVLTreeInsert(_1000w, _avl);
        }

        [Benchmark]
        public void RBTreeRemove10W()
        {
            RBTreeRemove(_rnd10W, _set);
        }

        [Benchmark]
        public void RBTreeRemove100W()
        {
            RBTreeRemove(_rnd100W, _set);
        }

        [Benchmark]
        public void AVLTreeRemove10W()
        {
            AVLTreeRemove(_rnd10W, _avl);
        }

        [Benchmark]
        public void AVLTreeRemove100W()
        {
            AVLTreeRemove(_rnd100W, _avl);
        }
    }

    public abstract class BenchmarkAvlSet
    {
        protected readonly int[] _10w;
        protected readonly int[] _100w;

        protected SortedSet<int> _10wSet;
        protected SortedSet<int> _100wSet;

        protected AvlTree<int> _10wAvl;
        protected AvlTree<int> _100wAvl;

        public BenchmarkAvlSet()
        {
            _10w = Gen(100_000);
            _100w = Gen(1_000_000);
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

        public void RBTreeInsert(int[] data, SortedSet<int> set)
        {
            foreach (var i in data)
            {
                set.Add(i);
            }
        }

        public void AVLTreeInsert(int[] data, AvlTree<int> avl)
        {
            foreach (var i in data)
            {
                avl.Add(i);
            }
        }

        public void RBTreeFind(int[] order, SortedSet<int> set)
        {
            foreach (var i in order)
            {
                _ = set.Contains(i);
            }
        }

        public void AVLTreeFind(int[] order, AvlTree<int> set)
        {
            foreach (var i in order)
            {
                _ = set.Contains(i);
            }
        }

        public void RBTreeRemove(int[] order, SortedSet<int> set)
        {
            foreach (var i in order)
            {
                set.Remove(i);
            }
        }

        public void AVLTreeRemove(int[] order, AvlTree<int> set)
        {
            foreach (var i in order)
            {
                set.Remove(i);
            }
        }
    }
}