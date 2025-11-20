using System;
using System.Runtime.InteropServices;

namespace Hydra.Sdk.Extensions;

public static class ObjectExtensions
{
	public static byte[] ToBytes(this object obj)
	{
		int num = Marshal.SizeOf(obj);
		byte[] array = new byte[num];
		IntPtr intPtr = Marshal.AllocHGlobal(num);
		Marshal.StructureToPtr(obj, intPtr, fDeleteOld: false);
		Marshal.Copy(intPtr, array, 0, num);
		Marshal.FreeHGlobal(intPtr);
		return array;
	}

	public static string GetLogCatMsg(this object obj)
	{
		return "SDK/Log/" + obj.GetType().Name;
	}

	public static string GetLogCatInf(this object obj)
	{
		return "SDK/Info/" + obj.GetType().Name;
	}

	public static string GetLogCatWrn(this object obj)
	{
		return "SDK/Warning/" + obj.GetType().Name;
	}

	public static string GetLogCatErr(this object obj)
	{
		return "SDK/Error/" + obj.GetType().Name;
	}
}
