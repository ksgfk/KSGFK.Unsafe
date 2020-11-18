using System;
using System.Numerics;
using System.Runtime.Intrinsics;
using KSGFK.Collections;
using KSGFK.Unsafe;
using NUnit.Framework;

namespace Test
{
    public class AllTest
    {
        struct TestSizeOf
        {
            int i4;
            float f4;
        }

        //TODO:严格测试
        [Test]
        public void SimpleTest()
        {
            TestEmitUnsafeSizeOf();
            TestNativeArray();
            TestNativeList();
            TestNativePriorityQueue();
            TestPriorityQueue();
            TestNaiveQuickSort();
            Console.WriteLine("通过测试");
        }

        static unsafe void TestEmitUnsafeSizeOf()
        {
            if (Unsafe.SizeOf<int>() != sizeof(int)) throw new Exception();
            if (Unsafe.SizeOf<float>() != sizeof(float)) throw new Exception();
            if (Unsafe.SizeOf<double>() != sizeof(double)) throw new Exception();
            if (Unsafe.SizeOf<byte>() != sizeof(byte)) throw new Exception();
            if (Unsafe.SizeOf<Vector3>() != sizeof(Vector3)) throw new Exception();
            if (Unsafe.SizeOf<Vector128<int>>() != sizeof(Vector128<int>)) throw new Exception();
            if (Unsafe.SizeOf<Vector64<float>>() != sizeof(Vector64<float>)) throw new Exception();
            if (Unsafe.SizeOf<Vector256<double>>() != sizeof(Vector256<double>)) throw new Exception();
            if (Unsafe.SizeOf<TestSizeOf>() != sizeof(TestSizeOf)) throw new Exception();
        }

        static unsafe void TestNaiveQuickSort()
        {
            const int cnt = 100;
            var rand = new Random((int) DateTime.Now.Ticks);
            using var arr = new NativeArray<int>(cnt, 0);
            var end = new int[cnt];
            for (int i = 0; i < cnt; i++)
            {
                var r = rand.Next();
                arr[i] = r;
                end[i] = r;
            }

            // var span = new Span<int>(arr.Ptr, cnt);
            arr.Sort();
            Array.Sort(end);

            for (int i = 0; i < cnt; i++)
            {
                if (arr[i] != end[i]) throw new ArgumentException();
                // Console.WriteLine(arr[i]);
            }
        }

        static unsafe void TestPriorityQueue()
        {
            const int count = 100;
            var data = new int[count];
            var rand = new Random((int) DateTime.Now.Ticks);
            for (var i = count - 1; i >= 0; i--)
            {
                data[i] = rand.Next();
            }

            var q = new PriorityQueue<int>();
            // var span = new Span<int>((int*) q.Ptr, count + 1);
            for (int i = 0; i < count; i++)
            {
                q.Enqueue(data[i]);
            }

            Array.Sort(data);
            var t = 0;
            while (!q.IsEmpty)
            {
                if (q.Peek() != data[t++]) throw new ArgumentException();
                // Console.WriteLine(q.Peek());
                q.Dequeue();
            }
        }

        static unsafe void TestNativePriorityQueue()
        {
            const int count = 100;
            var data = new int[count];
            var rand = new Random((int) DateTime.Now.Ticks);
            for (var i = count - 1; i >= 0; i--)
            {
                data[i] = rand.Next();
            }

            using var q = new NativePriorityQueue<int>(count, 1);
            // var span = new Span<int>((int*) q.Ptr, count + 1);
            for (int i = 0; i < count; i++)
            {
                q.Enqueue(data[i]);
            }

            Array.Sort(data);
            var t = 0;
            while (!q.IsEmpty)
            {
                if (q.Peek() != data[t++]) throw new ArgumentException();
                // Console.WriteLine(q.Peek());
                q.Dequeue();
            }
        }

        static void TestNativeArray()
        {
            const int count = 100;
            using var arr = new NativeArray<int>(count, 1);
            for (var i = 0; i < arr.Count; i++)
            {
                arr[i] = i;
            }

            if (arr.Count != count) throw new ArgumentException();
            for (var i = 0; i < arr.Count; i++)
            {
                if (arr[i] != i) throw new ArgumentException($"{i}");
            }

            var cpy = new int[101];
            arr.CopyTo(cpy, 1);
            for (int i = 0; i < arr.Count; i++)
            {
                if (arr[i] != cpy[i + 1]) throw new ArgumentException($"{i}");
            }

            for (int i = 0; i < arr.Count; i++)
            {
                if (arr.IndexOf(i) == -1) throw new ArgumentException($"{i}");
            }

            for (int i = 0; i < arr.Count; i++)
            {
                if (!arr.Contains(i)) throw new ArgumentException($"{i}");
            }
        }

        static void TestNativeList()
        {
            const int count = 100;
            using var list = new NativeList<int>(count, 1);

            //Add
            for (var i = 0; i < list.Capacity; i++)
            {
                list.Add(i);
            }

            //Count
            if (list.Count != count) throw new ArgumentOutOfRangeException($"count:{list.Count}");
            //Capacity
            if (list.Capacity != count) throw new ArgumentOutOfRangeException($"capacity:{list.Capacity}");

            //Value
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] != i) throw new ArgumentException($"{i}");
            }

            //CopyTo
            var cpy = new int[101];
            list.CopyTo(cpy, 1);
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] != cpy[i + 1]) throw new ArgumentException($"{i}");
            }

            //IndexOf
            for (var i = 0; i < list.Count; i++)
            {
                if (list.IndexOf(i) == -1) throw new ArgumentException($"{i}");
            }

            //Contains
            for (var i = 0; i < list.Count; i++)
            {
                if (!list.Contains(i)) throw new ArgumentException($"{i}");
            }

            //RemoveAt
            list.RemoveAt(count / 2);
            if (list.Count != count - 1) throw new ArgumentOutOfRangeException($"count:{list.Count}");

            for (var i = count / 2; i < list.Count; i++)
            {
                if (list[i] != i + 1) throw new ArgumentException($"{list[i]}");
            }

            //Insert
            list.Insert(count / 2, count / 2);
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] != i) throw new ArgumentException($"{i}");
            }

            //Remove
            list.Remove(0);
            if (list[0] != 1) throw new ArgumentException("0");

            list.Clear();
            if (list.Count != 0) throw new ArgumentOutOfRangeException($"count:{list.Count}");
            try
            {
                Console.WriteLine(list[0]);
            }
            catch (IndexOutOfRangeException)
            {
            }
        }
    }
}