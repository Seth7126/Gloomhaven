using System;
using System.Runtime.InteropServices;
using AOT;

public class SystemMemory
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct MemCounters
	{
		public long currentMemoryAllocatedInBytes;
	}

	public delegate IntPtr EOS_GenericAlignAlloc(UIntPtr sizeInBytes, UIntPtr alignmentInBytes);

	public delegate IntPtr EOS_GenericAlignRealloc(IntPtr ptr, UIntPtr sizeInBytes, UIntPtr alignmentInBytes);

	public delegate void EOS_GenericFree(IntPtr ptr);

	private const string DLLHBinaryName = "DynamicLibraryLoaderHelper";

	[MonoPInvokeCallback(typeof(EOS_GenericAlignAlloc))]
	public static IntPtr GenericAlignAlloc(UIntPtr sizeInBytes, UIntPtr alignmentInBytes)
	{
		return Mem_generic_align_alloc(sizeInBytes, alignmentInBytes);
	}

	[MonoPInvokeCallback(typeof(EOS_GenericAlignRealloc))]
	public static IntPtr GenericAlignRealloc(IntPtr ptr, UIntPtr sizeInBytes, UIntPtr alignmentInBytes)
	{
		return Mem_generic_align_realloc(ptr, sizeInBytes, alignmentInBytes);
	}

	[MonoPInvokeCallback(typeof(EOS_GenericFree))]
	public static void GenericFree(IntPtr ptr)
	{
		Mem_generic_free(ptr);
	}

	public static void GetAllocatorFunctions(out IntPtr alloc, out IntPtr realloc, out IntPtr free)
	{
		alloc = IntPtr.Zero;
		realloc = IntPtr.Zero;
		free = IntPtr.Zero;
	}

	[DllImport("DynamicLibraryLoaderHelper")]
	public static extern IntPtr Mem_generic_align_alloc(UIntPtr size_in_bytes, UIntPtr alignment_in_bytes);

	[DllImport("DynamicLibraryLoaderHelper")]
	public static extern IntPtr Mem_generic_align_realloc(IntPtr ptr, UIntPtr size_in_bytes, UIntPtr alignment_in_bytes);

	[DllImport("DynamicLibraryLoaderHelper")]
	public static extern void Mem_generic_free(IntPtr ptr);
}
