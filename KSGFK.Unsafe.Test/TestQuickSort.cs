using System;
using System.Collections.Generic;
using KSGFK.Unsafe;
using NUnit.Framework;

namespace Test
{
    public class TestQuickSort
    {
        [Test]
        public unsafe void Setup()
        {
            const int cnt = 1000;
            var rand = new Random();
            var raw = new int[cnt];
            for (var index = 0; index < raw.Length; index++)
            {
                raw[index] = rand.Next(cnt);
            }

            using var arr = new NativeArray<int>(0, cnt);
            for (var index = 0; index < arr.Count; index++)
            {
                arr[index] = raw[index];
            }

            Array.Sort(raw);
            Unsafe.QuickSort(arr.Ptr, arr.Count, sizeof(int), Comparer<int>.Default);
            for (var i = 0; i < cnt; i++)
            {
                Assert.True(raw[i] == arr[i]);
            }
        }
    }
}