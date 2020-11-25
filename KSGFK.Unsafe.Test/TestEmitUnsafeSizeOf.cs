using System.Numerics;
using System.Runtime.Intrinsics;
using KSGFK.Unsafe;
using NUnit.Framework;

namespace Test
{
    public class TestEmitUnsafeSizeOf
    {
        struct TestSizeOf
        {
            int i4;
            float f4;
        }

        [Test]
        public unsafe void Test()
        {
            Assert.True(Unsafe.SizeOf<int>() == sizeof(int));
            Assert.True(Unsafe.SizeOf<float>() == sizeof(float));
            Assert.True(Unsafe.SizeOf<double>() == sizeof(double));
            Assert.True(Unsafe.SizeOf<byte>() == sizeof(byte));
            Assert.True(Unsafe.SizeOf<Vector3>() == sizeof(Vector3));
            Assert.True(Unsafe.SizeOf<Vector128<int>>() == sizeof(Vector128<int>));
            Assert.True(Unsafe.SizeOf<Vector64<float>>() == sizeof(Vector64<float>));
            Assert.True(Unsafe.SizeOf<Vector256<double>>() == sizeof(Vector256<double>));
            Assert.True(Unsafe.SizeOf<TestSizeOf>() == sizeof(TestSizeOf));
        }
    }
}