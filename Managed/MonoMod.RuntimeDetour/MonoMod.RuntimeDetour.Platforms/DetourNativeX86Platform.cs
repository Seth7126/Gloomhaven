using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MonoMod.RuntimeDetour.Platforms;

public sealed class DetourNativeX86Platform : IDetourNativePlatform
{
	public enum DetourType : byte
	{
		Rel32,
		Abs32,
		Abs64
	}

	private static readonly uint[] DetourSizes = new uint[3] { 5u, 6u, 14u };

	private static bool Is32Bit(long to)
	{
		return (to & 0xFFFFFFFFu) == to;
	}

	private static DetourType GetDetourType(IntPtr from, IntPtr to)
	{
		long num = (long)to - ((long)from + 5);
		if (Is32Bit(num) || Is32Bit(-num))
		{
			return DetourType.Rel32;
		}
		if (Is32Bit((long)to))
		{
			return DetourType.Abs32;
		}
		return DetourType.Abs64;
	}

	public NativeDetourData Create(IntPtr from, IntPtr to, byte? type)
	{
		NativeDetourData result = new NativeDetourData
		{
			Method = from,
			Target = to
		};
		uint[] detourSizes = DetourSizes;
		int num = ((int?)type) ?? ((int)GetDetourType(from, to));
		byte b = (byte)num;
		result.Type = (byte)num;
		result.Size = detourSizes[b];
		return result;
	}

	public void Free(NativeDetourData detour)
	{
	}

	public void Apply(NativeDetourData detour)
	{
		int offs = 0;
		switch ((DetourType)detour.Type)
		{
		case DetourType.Rel32:
			detour.Method.Write(ref offs, 233);
			detour.Method.Write(ref offs, (uint)((long)detour.Target - ((long)detour.Method + offs + 4)));
			break;
		case DetourType.Abs32:
			detour.Method.Write(ref offs, 104);
			detour.Method.Write(ref offs, (uint)(int)detour.Target);
			detour.Method.Write(ref offs, 195);
			break;
		case DetourType.Abs64:
			detour.Method.Write(ref offs, byte.MaxValue);
			detour.Method.Write(ref offs, 37);
			detour.Method.Write(ref offs, 0u);
			detour.Method.Write(ref offs, (ulong)(long)detour.Target);
			break;
		default:
			throw new NotSupportedException($"Unknown detour type {detour.Type}");
		}
	}

	public unsafe void Copy(IntPtr src, IntPtr dst, byte type)
	{
		switch ((DetourType)type)
		{
		case DetourType.Rel32:
			*(int*)(long)dst = *(int*)(long)src;
			*(sbyte*)((long)dst + 4) = *(sbyte*)((long)src + 4);
			break;
		case DetourType.Abs32:
			*(int*)(long)dst = *(int*)(long)src;
			*(short*)((long)dst + 4) = *(short*)((long)src + 4);
			break;
		case DetourType.Abs64:
			*(long*)(long)dst = *(long*)(long)src;
			*(int*)((long)dst + 8) = *(int*)((long)src + 8);
			*(short*)((long)dst + 12) = *(short*)((long)src + 12);
			break;
		default:
			throw new NotSupportedException($"Unknown detour type {type}");
		}
	}

	public void MakeWritable(IntPtr src, uint size)
	{
	}

	public void MakeExecutable(IntPtr src, uint size)
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void FlushICache(IntPtr src, uint size)
	{
	}

	public IntPtr MemAlloc(uint size)
	{
		return Marshal.AllocHGlobal((int)size);
	}

	public void MemFree(IntPtr ptr)
	{
		Marshal.FreeHGlobal(ptr);
	}
}
