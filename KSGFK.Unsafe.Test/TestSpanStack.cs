using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace KSGFK.Unsafe.Test
{
    public class TestSpanStack
    {
        [Test]
        public unsafe void Test()
        {
            const int cnt = 100;
            var stack = new SpanStack<int>(stackalloc int[cnt]);
            var managed = new Stack<int>(cnt);

            var arr = new int[cnt];
            var rand = new Random();
            for (int i = 0; i < cnt; i++)
            {
                arr[i] = rand.Next(cnt);
            }

            for (int i = 0; i < cnt; i++)
            {
                managed.Push(arr[i]);
                stack.Push(arr[i]);
            }

            var se = stack.GetEnumerator();
            var me = managed.GetEnumerator();
            for (var i = 0; i < cnt; i++)
            {
                se.MoveNext();
                me.MoveNext();
                Assert.True(se.Current == me.Current);
            }

            se.Dispose();
            me.Dispose();

            var toArr = stack.ToArray();
            var meArr = managed.ToArray();
            Assert.AreEqual(toArr.Length, meArr.Length);
            for (int i = 0; i < toArr.Length; i++)
            {
                Assert.AreEqual(toArr[i], meArr[i]);
            }

            for (int i = 0; i < cnt; i++)
            {
                Assert.True(managed.Count == stack.Count);
                var l = managed.Pop();
                var r = stack.Peek();
                Assert.True(l == r);
                stack.Pop();
            }

            stack.Clear();
            Assert.True(stack.Count == 0);
        }
    }
}
