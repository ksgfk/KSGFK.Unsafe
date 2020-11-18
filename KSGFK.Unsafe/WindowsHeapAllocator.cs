using System;
using System.Runtime.InteropServices;

namespace KSGFK.Unsafe
{
    public enum HeapFlag : ulong
    {
        None = 0,
        NoSerialize = 0x00000001,
        Growable = 0x00000002,
        GenerateExceptions = 0x00000004,
        ZeroMemory = 0x00000008,
        ReAllocInPlaceOnly = 0x00000010,
        TailCheckingEnabled = 0x00000020,
        FreeCheckingEnabled = 0x00000040,
        DisableCoalesceOnFree = 0x00000080,
        CreateAlign16 = 0x00010000,
        CreateEnableTracing = 0x00020000,
        CreateEnableExecute = 0x00040000,
        MaximumTag = 0x0FFF,
        PseudoTagFlag = 0x8000,
        TagShift = 18,
        CreateSegmentHeap = 0x00000100,
        CreateHardened = 0x00000200,
    }

    public unsafe class WindowsHeapAllocator : IAllocator
    {
        public static WindowsHeapAllocator Default { get; } = new WindowsHeapAllocator();

        [DllImport(Unsafe.NameKernel32)]
        public static extern void* HeapAlloc(void* handle, HeapFlag flag, UIntPtr size);

        [DllImport(Unsafe.NameKernel32)]
        public static extern bool HeapFree(void* handle, HeapFlag flag, void* ptr);

        [DllImport(Unsafe.NameKernel32)]
        public static extern void* HeapReAlloc(void* handle, HeapFlag flag, void* lpMem, UIntPtr size);

        [DllImport(Unsafe.NameKernel32)]
        public static extern void* GetProcessHeap();

        [DllImport(Unsafe.NameKernel32)]
        public static extern ulong GetLastError();

        private readonly void* _heapHandler;

        public WindowsHeapAllocator() { _heapHandler = GetProcessHeap(); }

        public void* Malloc(ulong size)
        {
            var r = HeapAlloc(_heapHandler, 0, new UIntPtr(size));
            if (r == null)
            {
                throw new OutOfMemoryException();
            }

            return r;
        }

        public void Free(void* ptr)
        {
            if (ptr == null)
            {
                throw new ArgumentException();
            }

            if (!HeapFree(_heapHandler, 0, ptr))
            {
                throw new InvalidOperationException($"free mem err code:{GetLastError()}");
            }
        }

        public void* ReAlloc(void* source, ulong newSize)
        {
            var ptr = HeapReAlloc(_heapHandler, 0, source, new UIntPtr(newSize));
            if (ptr == null)
            {
                throw new OutOfMemoryException();
            }

            return ptr;
        }
    }
}