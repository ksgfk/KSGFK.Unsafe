using KSGFK.Unsafe;
using NUnit.Framework;

namespace Test
{
    public class TestNativeArray
    {
        [Test]
        public void Test()
        {
            const int count = 100;
            using var arr = new NativeArray<int>(1, count);
            for (var i = 0; i < arr.Count; i++)
            {
                arr[i] = i;
            }

            Assert.True(arr.Count == count);
            for (var i = 0; i < arr.Count; i++)
            {
                Assert.True(arr[i] == i);
            }

            var cpy = new int[101];
            arr.CopyTo(cpy, 1);
            for (int i = 0; i < arr.Count; i++)
            {
                Assert.True(arr[i] == cpy[i + 1]);
            }

            for (int i = 0; i < arr.Count; i++)
            {
                Assert.True(arr.IndexOf(i) >= 0);
            }

            for (int i = 0; i < arr.Count; i++)
            {
                Assert.True(arr.Contains(i));
            }

            var span = arr.ToSpan();
            Assert.True(span.Length == arr.Count);
            for (int i = 0; i < span.Length; i++)
            {
                Assert.True(span[i] == arr[i]);
            }
        }
    }
}