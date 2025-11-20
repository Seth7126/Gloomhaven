using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MonoMod.RuntimeDetour.Platforms;

public sealed class DetourNativeWindowsPlatform : IDetourNativePlatform
{
	[Flags]
	private enum Protection : uint
	{
		PAGE_NOACCESS = 1u,
		PAGE_READONLY = 2u,
		PAGE_READWRITE = 4u,
		PAGE_WRITECOPY = 8u,
		PAGE_EXECUTE = 0x10u,
		PAGE_EXECUTE_READ = 0x20u,
		PAGE_EXECUTE_READWRITE = 0x40u,
		PAGE_EXECUTE_WRITECOPY = 0x80u,
		PAGE_GUARD = 0x100u,
		PAGE_NOCACHE = 0x200u,
		PAGE_WRITECOMBINE = 0x400u
	}

	private readonly IDetourNativePlatform Inner;

	public DetourNativeWindowsPlatform(IDetourNativePlatform inner)
	{
		Inner = inner;
	}

	public void MakeWritable(IntPtr src, uint size)
	{
		if (!VirtualProtect(src, (IntPtr)size, Protection.PAGE_EXECUTE_READWRITE, out var _))
		{
			throw new Win32Exception();
		}
	}

	public void MakeExecutable(IntPtr src, uint size)
	{
		if (!VirtualProtect(src, (IntPtr)size, Protection.PAGE_EXECUTE_READWRITE, out var _))
		{
			throw new Win32Exception();
		}
	}

	public void FlushICache(IntPtr src, uint size)
	{
		if (!FlushInstructionCache(GetCurrentProcess(), src, (UIntPtr)size))
		{
			throw new Win32Exception();
		}
	}

	public NativeDetourData Create(IntPtr from, IntPtr to, byte? type)
	{
		return Inner.Create(from, to, type);
	}

	public void Free(NativeDetourData detour)
	{
		Inner.Free(detour);
	}

	public void Apply(NativeDetourData detour)
	{
		Inner.Apply(detour);
	}

	public void Copy(IntPtr src, IntPtr dst, byte type)
	{
		Inner.Copy(src, dst, type);
	}

	public IntPtr MemAlloc(uint size)
	{
		return Inner.MemAlloc(size);
	}

	public void MemFree(IntPtr ptr)
	{
		Inner.MemFree(ptr);
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool VirtualProtect(IntPtr lpAddress, IntPtr dwSize, Protection flNewProtect, out Protection lpflOldProtect);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern IntPtr GetCurrentProcess();

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool FlushInstructionCache(IntPtr hProcess, IntPtr lpBaseAddress, UIntPtr dwSize);
}
