using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KSGFK.Unsafe
{
    public static unsafe class Unsafe
    {
        private class CSharpInteropAllocator : IAllocator
        {
            internal static CSharpInteropAllocator Default { get; } = new CSharpInteropAllocator();

            public void* Malloc(ulong size)
            {
                if (size > int.MaxValue) throw new ArgumentOutOfRangeException();
                return Marshal.AllocHGlobal((int) size).ToPointer();
            }

            public void Free(void* block) { Marshal.FreeHGlobal(new IntPtr(block)); }

            public void* ReAlloc(void* source, ulong newSize)
            {
                if (newSize > int.MaxValue) throw new ArgumentOutOfRangeException();
                return Marshal.ReAllocHGlobal(new IntPtr(source), (IntPtr) newSize).ToPointer();
            }
        }

        public static readonly List<IAllocator> Allocators;
        private static readonly MethodInfo EmitSizeOf;
        public const string NameKernel32 = "Kernel32.dll";
        public const string Namelibdl = "libdl.so";

        static Unsafe()
        {
            Allocators = new List<IAllocator>
            {
                CSharpInteropAllocator.Default,
                WindowsHeapAllocator.Default
            };

            var asmName = new AssemblyName("UnsafeExt");
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndCollect);
            var moduleBuilder = asmBuilder.DefineDynamicModule(asmName.Name);
            var typeBuilder = moduleBuilder.DefineType("UnsafeExt", TypeAttributes.Public);
            var methodBuilder = typeBuilder.DefineMethod("SizeOf",
                MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard,
                typeof(int),
                null);
            var genericBuilder = methodBuilder.DefineGenericParameters("T");
            var ilGen = methodBuilder.GetILGenerator();
            ilGen.Emit(OpCodes.Sizeof, genericBuilder[0]);
            ilGen.Emit(OpCodes.Ret);
            var extType = typeBuilder.CreateType();
            EmitSizeOf = extType.GetMethod("SizeOf", BindingFlags.Public | BindingFlags.Static);
        }

        public static void* AlignedMalloc(ulong size, int align, int allocator)
        {
            //https://stackoverflow.com/questions/38088732/explanation-to-aligned-malloc-implementation
            long offset = align - 1 + sizeof(void*);
            var p1 = Allocators[allocator].Malloc(size + (ulong) offset);
            if (p1 == null)
            {
                return null;
            }

            var p2 = (void**) (((long) p1 + offset) & ~(align - 1));
            p2[-1] = p1;
            return p2;
        }

        public static void AlignedFree(void* block, int allocator) { Allocators[allocator].Free(((void**) block)[-1]); }

        public static void* AlignedReAlloc(void* block, ulong newSize, int align, int allocator)
        {
            var raw = ((void**) block)[-1];
            var newPtr = Allocators[allocator].ReAlloc(raw, newSize);
            if (newPtr == raw)
            {
                return block;
            }

            long offset = align - 1 + sizeof(void*);
            var p2 = (void**) (((long) newPtr + offset) & ~(align - 1));
            p2[-1] = newPtr;
            return p2;
        }

        public static void* Malloc(ulong size, int allocator)
        {
            var align = size % 16;
            return AlignedMalloc(size, align == 0 ? 16 : (int) align, allocator);
        }

        public static void Free(void* block, int allocator) { AlignedFree(block, allocator); }

        public static void* ReAlloc(void* block, ulong newSize, int allocator)
        {
            var align = newSize % 16;
            return AlignedReAlloc(block, newSize, align == 0 ? 16 : (int) align, allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetArrayItem<T>(void* source, int index) where T : unmanaged
        {
            return ref *(T*) ((byte*) source + (long) index * (long) sizeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetArrayItem<T>(void* source, int size, int index) where T : struct
        {
            return ref MemoryMarshal.GetReference(new ReadOnlySpan<T>((byte*) source + (long) index * size, 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetArrayItem<T>(void* arr, int index, ref T item) where T : unmanaged
        {
            *(T*) ((byte*) arr + (long) index * (long) sizeof(T)) = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetArrayItem<T>(void* arr, int size, int index, ref T item) where T : struct
        {
            MemoryMarshal.Write(new Span<byte>((byte*) arr + (long) index * size, size), ref item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetArrayItem(void* arr, int size, int index, void* item)
        {
            CopyData(item, 0, arr, index, 1, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetItemIndex<T>(void* arr, int size, int length, in T item, IEqualityComparer<T> cmp)
            where T : struct
        {
            for (var i = 0; i < length; i++)
            {
                if (cmp.Equals(item, GetArrayItem<T>(arr, size, i)))
                {
                    return i;
                }
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetItemIndex<T>(void* arr, int size, int length, in T item) where T : struct
        {
            var cmp = EqualityComparer<T>.Default;
            for (var i = 0; i < length; i++)
            {
                if (cmp.Equals(item, GetArrayItem<T>(arr, size, i)))
                {
                    return i;
                }
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyData<T>(T* src, int srcStart, T* dst, int dstStart, int count) where T : unmanaged
        {
            var s = new Span<T>(src + srcStart, count);
            var d = new Span<T>(dst + dstStart, count);
            s.CopyTo(d);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyData<T>(void* src, int srcStart, T[] dst, int dstStart, int count, int size)
            where T : struct
        {
            var s = MemoryMarshal.Cast<byte, T>(new Span<byte>((byte*) src + srcStart * size, count * size));
            var d = new Span<T>(dst, dstStart, count);
            s.CopyTo(d);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyData(void* src, int srcStart, void* dst, int dstStart, int count, int size)
        {
            var s = new Span<byte>((byte*) src + srcStart * size, count * size);
            var d = new Span<byte>((byte*) dst + dstStart * size, count * size);
            s.CopyTo(d);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear(void* src, int count, int size) { new Span<byte>(src, count * size).Fill(0); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear<T>(T* src, int len) where T : unmanaged { new Span<T>(src, len).Fill(default); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(ref T l, ref T r)
        {
            var t = l;
            l = r;
            r = t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap(void* l, void* r, int size)
        {
            if (l == r) return;
            Span<byte> temp = stackalloc byte[size];
            var lSpan = new Span<byte>(l, size);
            var rSpan = new Span<byte>(r, size);
            lSpan.CopyTo(temp);
            rSpan.CopyTo(lSpan);
            temp.CopyTo(rSpan);
        }

        public static int SizeOf<T>() where T : struct
        {
            var genericMethod = EmitSizeOf.MakeGenericMethod(typeof(T));
            return (int) genericMethod.Invoke(null, null);
        }

        public static void QuickSort<T>(void* src, int count, int size, IComparer<T> cmp) where T : struct
        {
            using var stack = new NativeStack<int>(0, 256);
            stack.Push(0);
            stack.Push(count - 1);
            var biSrc = (byte*) src;

            while (stack.Count > 0)
            {
                var right = stack.Peek();
                stack.Pop();
                var left = stack.Peek();
                stack.Pop();
                var mid = GetArrayItem<T>(src, size, (left + right) / 2);
                int i = left, j = right;
                do
                {
                    while (cmp.Compare(GetArrayItem<T>(src, size, i), mid) < 0) i++;
                    while (cmp.Compare(GetArrayItem<T>(src, size, j), mid) > 0) j--;
                    if (i <= j)
                    {
                        Swap(biSrc + i * size, biSrc + j * size, size);
                        i++;
                        j--;
                    }
                } while (i <= j);

                if (left < j)
                {
                    stack.Push(left);
                    stack.Push(j);
                }

                if (i < right)
                {
                    stack.Push(i);
                    stack.Push(right);
                }
            }
        }

        public static void QuickSort<T>(void* src, int left, int right, int size, IComparer<T> cmp) where T : struct
        {
            if (left < 0) throw new ArgumentOutOfRangeException();
            if (right < 0) throw new ArgumentOutOfRangeException();
            if (right - left < 0) throw new ArgumentOutOfRangeException();
            var biSrc = (byte*) src;
            //https://github.com/MaxwellGengYF/Unity-MPipeline
            int i = left, j = right;
            var temp = GetArrayItem<T>(src, size, left);
            while (i < j)
            {
                while (cmp.Compare(GetArrayItem<T>(biSrc, size, j), temp) >= 0 && j > i) j--;
                if (j > i)
                {
                    SetArrayItem(biSrc, size, i, ref GetArrayItem<T>(biSrc, size, j));
                    i++;
                    while (cmp.Compare(GetArrayItem<T>(biSrc, size, i), temp) <= 0 && i < j) i++;
                    if (i < j)
                    {
                        SetArrayItem(biSrc, size, j, ref GetArrayItem<T>(biSrc, size, i));
                        j--;
                    }
                }
            }

            SetArrayItem(biSrc, size, i, ref temp);
            if (left < i - 1) QuickSort(src, left, i - 1, size, cmp);
            if (j + 1 < right) QuickSort(src, j + 1, right, size, cmp);
        }

        #region HeapOp

        //从MSVC里爬的STL源码(doge

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushHeap<T>(void* first, void* last, int size, IComparer<T> pred) where T : struct
        {
            var uFirst = (byte*) first;
            var uLast = (byte*) last;
            var count = (uLast - uFirst) / size;
            if (count < 0 || count >= int.MaxValue) throw new IndexOutOfRangeException();
            if (count >= 2)
            {
                uLast -= size;
                var val = uLast;
                var cnt = (int) count;
                PushHeapByIndex(uFirst, --cnt, 0, val, size, pred);
            }
        }

        public static void PushHeap<T>(IList<T> list, IComparer<T> pred)
        {
            var count = list.Count;
            if (count < 0 || count >= int.MaxValue) throw new IndexOutOfRangeException();
            if (count >= 2)
            {
                var uLast = count - 1;
                var val = list[uLast];
                PushHeapByIndex(list, uLast, 0, val, pred);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PushHeapByIndex<T>(void* first, int hole, int top, void* value, int size, IComparer<T> pred)
            where T : struct
        {
            var val = GetArrayItem<T>(value, size, 0);
            for (var idx = (hole - 1) >> 1;
                top < hole && pred.Compare(val, GetArrayItem<T>(first, size, idx)) < 0;
                idx = (hole - 1) >> 1)
            {
                SetArrayItem(first, size, hole, ref GetArrayItem<T>(first, size, idx));
                hole = idx;
            }

            SetArrayItem(first, size, hole, ref val);
        }

        private static void PushHeapByIndex<T>(IList<T> list, int hole, int top, T value, IComparer<T> pred)
        {
            for (var idx = (hole - 1) >> 1;
                top < hole && pred.Compare(value, list[idx]) < 0;
                idx = (hole - 1) >> 1)
            {
                list[hole] = list[idx];
                hole = idx;
            }

            list[hole] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PopHeap<T>(void* first, void* last, int size, IComparer<T> pred)
            where T : struct
        {
            var uLast = (byte*) last;
            var uFirst = (byte*) first;
            var count = (uLast - uFirst) / size;
            if (count < 0 || count >= int.MaxValue) throw new IndexOutOfRangeException();
            if (count >= 2)
            {
                uLast -= size;
                var val = stackalloc byte[size];
                var valSpan = new Span<byte>(val, size);
                var lastItem = new Span<byte>(uLast, size);
                lastItem.CopyTo(valSpan);
                SetArrayItem(uLast, size, 0, ref GetArrayItem<T>(uFirst, size, 0));
                var hole = 0;
                var bottom = (int) (uLast - uFirst) / size;
                var top = hole;
                var idx = hole;
                var maxSequenceNonLeaf = (bottom - 1) >> 1;
                while (idx < maxSequenceNonLeaf)
                {
                    idx = 2 * idx + 2;
                    if (pred.Compare(GetArrayItem<T>(uFirst, size, idx - 1), GetArrayItem<T>(uFirst, size, idx)) < 0)
                    {
                        --idx;
                    }

                    SetArrayItem(uFirst, size, hole, ref GetArrayItem<T>(uFirst, size, idx));
                    hole = idx;
                }

                if (idx == maxSequenceNonLeaf && bottom % 2 == 0)
                {
                    SetArrayItem(uFirst, size, hole, ref GetArrayItem<T>(uFirst, size, bottom - 1));
                    hole = bottom - 1;
                }

                PushHeapByIndex(uFirst, hole, top, val, size, pred);
            }
        }

        public static void PopHeap<T>(IList<T> list, IComparer<T> pred)
        {
            var count = list.Count;
            if (count < 0 || count >= int.MaxValue) throw new IndexOutOfRangeException();
            if (count >= 2)
            {
                var uLast = count - 1;
                var val = list[uLast];
                list[uLast] = list[0];
                var hole = 0;
                var bottom = uLast;
                var top = hole;
                var idx = hole;
                var maxSequenceNonLeaf = (bottom - 1) >> 1;
                while (idx < maxSequenceNonLeaf)
                {
                    idx = 2 * idx + 2;
                    if (pred.Compare(list[idx - 1], list[idx]) < 0)
                    {
                        --idx;
                    }

                    list[hole] = list[idx];
                    hole = idx;
                }

                if (idx == maxSequenceNonLeaf && bottom % 2 == 0)
                {
                    list[hole] = list[bottom - 1];
                    hole = bottom - 1;
                }

                PushHeapByIndex(list, hole, top, val, pred);
            }
        }

        #endregion

        [DllImport(NameKernel32, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibraryW([MarshalAs(UnmanagedType.LPWStr)] string lpLibFileName);

        [DllImport(Namelibdl)]
        public static extern IntPtr dlopen(string fileName, int flags);
    }
}