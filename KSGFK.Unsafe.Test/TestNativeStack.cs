using System;
using System.Collections.Generic;
using KSGFK.Unsafe;
using NUnit.Framework;

namespace Test
{
    public class TestNativeStack
    {
        [Test]
        public unsafe void Test()
        {
            const int cnt = 1000;
            using var stack = new NativeStack<int>(0);
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

            for (int i = 0; i < cnt; i++)
            {
                Assert.True(managed.Count == stack.Count);
                var l = managed.Pop();
                var r = stack.Peek();
                Assert.True(l == r);
                stack.Pop();
            }
            
            stack.Clear();
            stack.TrimExcess();

            Assert.True(!stack.IsDisposed);

            stack.Dispose();

            Assert.True(stack.Ptr == null);
            Assert.True(stack.IsDisposed);
        }
    }
}