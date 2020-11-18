namespace KSGFK.Unsafe
{
    public unsafe interface IAllocator
    {
        void* Malloc(ulong size);

        void Free(void* block);

        void* ReAlloc(void* source, ulong newSize);
    }
}