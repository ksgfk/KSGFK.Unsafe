using System;
using NUnit.Framework;

namespace KSGFK.Unsafe.Test
{
    public class TestPriorityQueue
    {
        [Test]
        public void TestManaged()
        {
            const int count = 100;
            var data = new int[count];
            var rand = new Random((int)DateTime.Now.Ticks);
            for (var i = count - 1; i >= 0; i--)
            {
                data[i] = rand.Next();
            }

            var q = new PriorityQueue<int>();
            for (int i = 0; i < count; i++)
            {
                q.Enqueue(data[i]);
                Assert.True(q.Count == i + 1);
            }

            Array.Sort(data);
            var t = 0;
            while (!q.IsEmpty)
            {
                Assert.True(q.Peek() == data[t++]);
                q.Dequeue();
            }
        }
    }
}