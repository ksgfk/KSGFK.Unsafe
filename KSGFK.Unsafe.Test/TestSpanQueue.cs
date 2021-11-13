using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace KSGFK.Unsafe.Test
{
    public class TestSpanQueue
    {
        [Test]
        public unsafe void Test()
        {
            const int cnt = 1000;
            var queue = new SpanQueue<int>(stackalloc int[cnt]);
            var managed = new Queue<int>(cnt);

            var arr = new int[cnt];
            var rand = new Random();
            for (int i = 0; i < cnt; i++)
            {
                arr[i] = rand.Next(cnt);
            }

            for (int i = 0; i < cnt; i++)
            {
                managed.Enqueue(arr[i]);
                queue.Enqueue(arr[i]);
            }

            var se = queue.GetEnumerator();
            var me = managed.GetEnumerator();
            var toArr = queue.ToArray();
            for (var i = 0; i < cnt; i++)
            {
                se.MoveNext();
                me.MoveNext();
                Assert.True(se.Current == me.Current);
                Assert.True(toArr[i] == se.Current);
            }

            se.Dispose();
            me.Dispose();

            for (int i = 0; i < cnt; i++)
            {
                Assert.True(managed.Count == queue.Count);
                var l = managed.Dequeue();
                var r = queue.Peek();
                Assert.True(l == r);
                queue.Dequeue();
            }

            queue.Clear();
            Assert.True(queue.Count == 0);
        }
    }
}
