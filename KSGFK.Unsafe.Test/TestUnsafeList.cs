using KSGFK.Unsafe;
using NUnit.Framework;

namespace Test
{
    public class TestUnsafeList
    {
        private readonly int _cap = 100;
        private UnsafeList _l;

        [SetUp]
        public void Init() { _l = new UnsafeList(sizeof(int), _cap, 0); }

        [Test]
        public unsafe void Test()
        {
            var span = _l.GetSpan<int>();
            Assert.True(span.Length == _cap);
            for (var i = 0; i < _l.Capacity; i++)
            {
                _l.Add(i);
            }

            var cnt = 0;
            foreach (var p in _l)
            {
                var val = (int*) p;
                Assert.True(*val == cnt);
                cnt++;
            }
        }
    }
}