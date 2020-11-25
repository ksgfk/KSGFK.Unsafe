using System;
using KSGFK.Unsafe;
using NUnit.Framework;

namespace Test
{
    public class TestNativeList
    {
        [Test]
        public void Test()
        {
            const int count = 100;
            using var list = new NativeList<int>(1, count);

            //Add
            for (var i = 0; i < list.Capacity; i++)
            {
                list.Add(i);
            }

            //Count
            Assert.True(list.Count == count);
            //Capacity
            Assert.True(list.Capacity == count);

            //Value
            for (var i = 0; i < list.Count; i++)
            {
                Assert.True(list[i] == i);
            }

            //CopyTo
            var cpy = new int[101];
            list.CopyTo(cpy, 1);
            for (var i = 0; i < list.Count; i++)
            {
                Assert.True(list[i] == cpy[i + 1]);
            }

            //IndexOf
            for (var i = 0; i < list.Count; i++)
            {
                Assert.True(list.IndexOf(i) >= 0);
            }

            //Contains
            for (var i = 0; i < list.Count; i++)
            {
                Assert.True(list.Contains(i));
            }

            //RemoveAt
            list.RemoveAt(count / 2);
            Assert.True(list.Count == count - 1);

            for (var i = count / 2; i < list.Count; i++)
            {
                Assert.True(list[i] == i + 1);
            }

            //Insert
            list.Insert(count / 2, count / 2);
            for (var i = 0; i < list.Count; i++)
            {
                Assert.True(list[i] == i);
            }

            var span = list.ToSpan();
            Assert.True(span.Length == list.Count);
            for (int i = 0; i < span.Length; i++)
            {
                Assert.True(list[i] == span[i]);
            }

            //Remove
            list.Remove(0);
            Assert.True(list[0] == 1);

            list.Clear();
            list.TrimExcess();
            Assert.True(list.Count == 0);
            Assert.True(list.Capacity == 0);
            Assert.Catch<IndexOutOfRangeException>(() => Console.WriteLine(list[0]));
        }
    }
}